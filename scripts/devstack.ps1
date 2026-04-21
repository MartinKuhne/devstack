param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("init", "run")]
    [string]$Command,

    [string]$ApiUrl = $env:DEVSTACK_API_URL
)

$ErrorActionPreference = 'Continue'

if ([string]::IsNullOrWhiteSpace($ApiUrl)) {
    $ApiUrl = "http://localhost:8087"
}

$AgentsFile  = Join-Path $PSScriptRoot "agents.md"

$GetProjectsQuery = @'
query GetProjects($first: Int!) {
  projects(first: $first) {
    nodes {
      id
      name
      description
      createdAt
    }
  }
}
'@

$CreateProjectMutation = @'
mutation CreateProject($name: String!, $description: String!) {
  createProject(input: { name: $name, description: $description }) {
    project {
      id
    }
    errors
  }
}
'@

function Test-GraphQLEndpoint {
    param([string]$Url)

    $body = @{ query = 'query { __schema { queryType { name } } }' } | ConvertTo-Json -Depth 10
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Post -Body $body -ContentType "application/json; charset=utf-8" -UseBasicParsing -ErrorAction Stop
        $result = $response | ConvertFrom-Json
        return -not $result.errors
    }
    catch { return $false }
}

function Resolve-GraphQLEndpoint {
    param([string]$PrimaryEndpoint, [string]$FallbackEndpoint)

    if (Test-GraphQLEndpoint -Url $PrimaryEndpoint) { return $PrimaryEndpoint }

    if ($FallbackEndpoint -and (Test-GraphQLEndpoint -Url $FallbackEndpoint)) {
        Write-Host "Using fallback GraphQL endpoint: $FallbackEndpoint"
        return $FallbackEndpoint
    }

    Write-Error "Unable to reach a valid GraphQL endpoint at '$PrimaryEndpoint' or '$FallbackEndpoint'."
    exit 1
}

$GraphQLEndpoint = Resolve-GraphQLEndpoint `
    -PrimaryEndpoint  "$($ApiUrl.TrimEnd('/'))/graphql" `
    -FallbackEndpoint "http://localhost:8087/graphql"

function Invoke-GraphQL {
    param([string]$Operation, [hashtable]$Variables = $null, [switch]$IsMutation)

    $body = @{ query = $Operation }
    if ($Variables) { $body.variables = $Variables }
    $jsonBody = $body | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-WebRequest -Uri $GraphQLEndpoint -Method Post -Body $jsonBody `
            -ContentType "application/json; charset=utf-8" -UseBasicParsing
        return $response | ConvertFrom-Json
    }
    catch {
        $responseBody = $null
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
        }
        $kind = if ($IsMutation) { "mutation" } else { "query" }
        Write-Error "GraphQL $kind failed: $_"
        if ($responseBody) { Write-Error "Response body: $responseBody" }
        throw
    }
}

function Get-GitRemoteOrigin {
    try {
        $originUrl = git remote get-url origin 2>$null
        if ([string]::IsNullOrWhiteSpace($originUrl)) {
            Write-Error "No git remote 'origin' configured"
            return $null
        }
        if ($originUrl -match 'github\.com[:/](?<org>[^/]+)/(?<repo>[^\.]+)') {
            return "$($matches.org)/$($matches.repo)"
        }
        if ($originUrl -match '/(?<repo>[^/]+?)(\.git)?$') {
            return $matches.repo
        }
        return $originUrl
    }
    catch {
        Write-Error "Failed to get git remote origin: $_"
        return $null
    }
}

function Initialize-Project {
    Write-Host "Initializing DevStack project..."

    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Write-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    Write-Host "Repository: $repoName"

    $result = Invoke-GraphQL -Operation $CreateProjectMutation -Variables @{ name = $repoName; description = 'Auto-initialized project' } -IsMutation

    if ($result.data.createProject.errors) {
        Write-Error "Failed to create project: $($result.data.createProject.errors -join ', ')"
        exit 1
    }

    $projectId = $result.data.createProject.project.id
    Write-Host "Project created with ID: $projectId"

    $opencodePath = Join-Path $PSScriptRoot "opencode.json"
    $existingConfig = $null
    if (Test-Path $opencodePath) {
        $existingConfig = Get-Content $opencodePath -Raw | ConvertFrom-Json
    }

    if (-not $existingConfig) {
        $existingConfig = [ordered]@{}
    }

    if ($existingConfig.PSObject.Properties['$schema'] -eq $null) {
        $configObj = @{}
        $configObj | Add-Member '$schema' "https://opencode.ai/config.json" -Force
        $existingConfig = $configObj
    }

    $mcpSection = $existingConfig.PSObject.Properties['mcp']
    if (-not $mcpSection) {
        $mcpSection = @{}
        $existingConfig | Add-Member 'mcp' $mcpSection -Force
    }

    $mcpSection.PSObject.Properties['devstack'] = @{
        type    = "remote"
        url     = "http://localhost:8088/mcp"
        enabled = $true
    }

    $existingConfig | ConvertTo-Json -Depth 10 | Set-Content $opencodePath
    Write-Host "Updated opencode.json"
    Write-Host "Initialization complete!"
}

function Get-CurrentProjectId {
    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Write-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    $result = Invoke-GraphQL -Operation $GetProjectsQuery -Variables @{ first = 100 }

    if ($result.errors) {
        Write-Error "Failed to query projects: $($result.errors -join ', ')"
        exit 1
    }

    $project = $result.data.projects.nodes | Where-Object { $_.name -eq $repoName }
    if (-not $project) {
        Write-Error "No project matching '$repoName' found. Run init first."
        exit 1
    }

    return $project.id
}

switch ($Command) {
    "init"  { Initialize-Project }
    "run"   { & (Join-Path $PSScriptRoot "agent.ps1") }
}

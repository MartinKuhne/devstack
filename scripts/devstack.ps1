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

$GetProjectQuery = @'
query GetProject($repoName: String) {
  projects(where: {  name: { eq: $repoName } } ) {
    nodes {
      id
      name
      description
      repository
    }
  }
}
'@

$CreateProjectMutation = @'
mutation CreateProject($input: CreateProjectInput!) {
  createProject(input: $input) {
    project {
      id
      name
      description
      repository
    }
    errors {
      field
      message
    }
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

        $result = $response | ConvertFrom-Json

        return $result
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

     # Check if project already exists
    $result = Invoke-GraphQL -Operation $GetProjectQuery -Variables @{ repoName = $repoName }

    $existingProject = $result.data.projects.nodes | Where-Object { $_.name -eq $repoName }

    if ($existingProject) {
        Write-Host "Project '$repoName' already exists with ID: $($existingProject.id)"
    }
    else {
        $result = Invoke-GraphQL -Operation $CreateProjectMutation -Variables @{ input = @{ name = $repoName; description = 'Auto-initialized project'; repository = $repoName } } -IsMutation

        if ($result.data.createProject.errors) {
            $errorMessages = $result.data.createProject.errors | ForEach-Object { "$($_.field): $($_.message)" }
            Write-Error "Failed to create project: $($errorMessages -join ', ')"
            exit 1
        }

        $existingProject = $result.data.createProject.project
        Write-Host "Project created with ID: $($existingProject.id)"
    }

    $opencodePath = Join-Path (Split-Path $PSScriptRoot -Parent) "opencode.json"
    $existingConfig = $null
    if (Test-Path $opencodePath) {
        $existingConfig = Get-Content $opencodePath -Raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    }

    if (-not $existingConfig) {
        $existingConfig = [ordered]@{}
    }

      if ($existingConfig.PSObject.Properties['$schema'] -eq $null) {
        $existingConfig | Add-Member '$schema' "https://opencode.ai/config.json" -Force
    }

    $mcpSection = $existingConfig.PSObject.Properties['mcp']
    $hasDevstackMcp = $false
    if ($mcpSection) {
        if ($mcpSection.PSObject.Properties['devstack']) {
            $hasDevstackMcp = $true
        }
    }

    if (-not $mcpSection) {
        $mcpSection = @{}
        $existingConfig | Add-Member 'mcp' $mcpSection -Force
    }

    if (-not $hasDevstackMcp) {
        $mcpSection['devstack'] = @{
            type    = "remote"
            url     = "http://localhost:8088/mcp"
            enabled = $true
        }
    }

    # REQ-AG-102: deny bash, question, external_directory permissions
    $permissionsSection = $existingConfig.PSObject.Properties['permissions']
    if (-not $permissionsSection) {
        $permissionsSection = @{}
        $existingConfig | Add-Member 'permissions' $permissionsSection -Force
    }

    $deniedPermissions = @('bash', 'question', 'external_directory')
    foreach ($perm in $deniedPermissions) {
        $permissionsSection[$perm] = 'deny'
    }

    $existingConfig | ConvertTo-Json -Depth 10 | Set-Content $opencodePath -Encoding UTF8
    Write-Host "Updated opencode.json"
    Write-Host "Initialization complete!"
}

function Get-CurrentProjectId {
    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
         Write-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
         exit 1
     }

    $result = Invoke-GraphQL -Operation $GetProjectQuery -Variables @{ repoName = $repoName}

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

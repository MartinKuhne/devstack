param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("init", "run")]
    [string]$Command,

    [string]$OpencodePrompt = '',

    [string]$ApiUrl = $env:DEVSTACK_API_URL
)

$ErrorActionPreference = 'Continue'

if ([string]::IsNullOrWhiteSpace($ApiUrl)) {
    $ApiUrl = "http://localhost:8087"
}

$QueriesPath = Join-Path $PSScriptRoot "queries"
$AgentsFile  = Join-Path $PSScriptRoot "agents.md"

function Load-GraphQLFile {
    param([string]$FileName)

    $path = Join-Path $QueriesPath $FileName
    if (-not (Test-Path $path)) {
        Write-Error "GraphQL file not found: $path"
        exit 1
    }
    return Get-Content -Path $path -Raw
}

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
    -FallbackEndpoint "http://localhost:5000/graphql"

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

    $mutation = Load-GraphQLFile "createProject.graphql"
    $result = Invoke-GraphQL -Operation $mutation -Variables @{ name = $repoName; description = 'Auto-initialized project' } -IsMutation

    if ($result.data.createProject.errors) {
        Write-Error "Failed to create project: $($result.data.createProject.errors -join ', ')"
        exit 1
    }

    $projectId = $result.data.createProject.project.id
    Write-Host "Project created with ID: $projectId"

    $config = [ordered]@{
        '$schema' = "https://opencode.ai/config.json"
        mcp = @{
            devstack = @{
                type    = "remote"
                url     = "http://localhost:8087/mcp"
                enabled = "true"
            }
        }
    }
    $config | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $PSScriptRoot "opencode.json")
    Write-Host "Created opencode.json"
    Write-Host "Initialization complete!"
}

function Get-CurrentProjectId {
    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Write-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    $query  = Load-GraphQLFile "getProjects.graphql"
    $result = Invoke-GraphQL -Operation $query -Variables @{ first = 100 }

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

function Invoke-AgentBatch {
    param(
        [string]   $EntityType,
        [string]   $QueryFile,
        [string]   $DataPath,
        [string]   $StatusFilter,
        [string]   $DefaultInstructions,
        [string[]] $ExtraFields = @()
    )

    $plural      = "${EntityType}s"
    $entityLabel = (Get-Culture).TextInfo.ToTitleCase($EntityType)
    Write-Host "Processing $plural in $StatusFilter status..."

    $projectId = Get-CurrentProjectId
    $query     = Load-GraphQLFile $QueryFile
    $result    = Invoke-GraphQL -Operation $query -Variables @{ projectId = $projectId }

    if ($result.errors) {
        Write-Error "Failed to query ${plural}: $($result.errors -join ', ')"
        exit 1
    }

    $items = $result.data.$DataPath.nodes | Where-Object { $_.status -eq $StatusFilter }

    if (-not $items) {
        Write-Host "No $plural in $StatusFilter status found for project."
        return
    }

    Write-Host "Found $($items.Count) ${EntityType}(s) in $StatusFilter status."

    foreach ($item in $items) {
        Write-Host "`nProcessing ${EntityType}: $($item.title) (ID: $($item.id))"

        $context = "$entityLabel ID: $($item.id)
Title: $($item.title)
Status: $($item.status)
Description: $($item.description)
AcceptanceCriteria: $($item.acceptanceCriteria)"

        foreach ($field in $ExtraFields) {
            $label    = $field -creplace '([A-Z])', ' $1'
            $context += "`n${label}: $($item.$field)"
        }

        $context += "`nPlan: $($item.plan)"

        $instructions = if ([string]::IsNullOrWhiteSpace($OpencodePrompt)) { $DefaultInstructions } else { $OpencodePrompt }
        $prompt = "$instructions`n`n$context"

        Write-Host $prompt

        $args = @("opencode", "run", $prompt)
        if (Test-Path $AgentsFile) { $args += @("--file", $AgentsFile) }
        & npx @args

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Opencode returned non-zero exit code for ${EntityType} $($item.id)"
        }

        Start-Sleep -Seconds 2
    }

    Write-Host "`nAll $plural processed."
}

switch ($Command) {
    "init" {
        Initialize-Project
    }
    "run" {
        Invoke-AgentBatch `
            -EntityType          "defect" `
            -QueryFile           "getDefects.graphql" `
            -DataPath            "defects" `
            -StatusFilter        "Planning" `
            -DefaultInstructions @"
Investigate the root cause for the failure. Reproduce it, collect logs/traces and metrics, identify the failing component and code path.
Propose a fix (if feasible within 5 minutes of research).
Use the update_defect tool to update the plan, securityImpact (if relevant), performanceImpact (if relevant), testPlan, deploymentPlan (if relevant), rootCause, openQuestions.
If there are no OpenQuestions, use the update_defect tool to change the state to Ready. If there are open questions, change the state to InReview.
"@

        Invoke-AgentBatch `
            -EntityType          "defect" `
            -QueryFile           "getDefects.graphql" `
            -DataPath            "defects" `
            -StatusFilter        "Ready" `
            -ExtraFields         @("RootCause") `
            -DefaultInstructions @"
Create a fix for this issue.
Quality gates must pass.
Commit the changes.
Use the update_defect tool to change the state to Done. If the operation was not successful, change the status to InReview instead.
"@

        Invoke-AgentBatch `
            -EntityType          "feature" `
            -QueryFile           "getFeatures.graphql" `
            -DataPath            "features" `
            -StatusFilter        "Planned" `
            -DefaultInstructions @"
Analyze the requirements for this feature. Break down the work, identify dependencies and risks.
Propose an implementation plan.
Use the update_feature tool to update the plan, securityImpact (if relevant), performanceImpact (if relevant), testPlan, deploymentPlan (if relevant), openQuestions.
If there are no OpenQuestions, use the update_feature tool to change the state to Analysis. If there are open questions, change the state to InReview.
"@

        Invoke-AgentBatch `
            -EntityType          "feature" `
            -QueryFile           "getFeatures.graphql" `
            -DataPath            "features" `
            -StatusFilter        "Analysis" `
            -DefaultInstructions @"
Implement this feature according to the plan.
Quality gates must pass.
Commit the changes.
Use the update_feature tool to change the state to Passed. If the operation was not successful, change the status to InReview instead.
"@
    }
}

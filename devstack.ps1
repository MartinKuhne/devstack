param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("init", "run")]
    [string]$Command,

    [string]$OpencodePrompt = ''
)

$ErrorActionPreference = 'Continue'

$ApiUrl = "http://localhost:8087"
$GraphQLEndpoint = "$ApiUrl/graphql"

function Invoke-GraphQLQuery {
    param(
        [string]$Query,
        [hashtable]$Variables = $null
    )
    
    $body = @{
        query = $Query
    }
    
    if ($Variables) {
        $body.variables = $Variables
    }
    
    $jsonBody = $body | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-WebRequest -Uri $GraphQLEndpoint -Method Post -Body $jsonBody -ContentType "application/json" -UseBasicParsing
        return $response.Content | ConvertFrom-Json
    }
    catch {
        Write-Error "GraphQL query failed: $_"
        throw
    }
}

function Invoke-GraphQLMutation {
    param(
        [string]$Mutation,
        [hashtable]$Variables = $null
    )
    
    $body = @{
        query = $Mutation
    }
    
    if ($Variables) {
        $body.variables = $Variables
    }
    
    $jsonBody = $body | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-WebRequest -Uri $GraphQLEndpoint -Method Post -Body $jsonBody -ContentType "application/json" -UseBasicParsing
        return $response.Content | ConvertFrom-Json
    }
    catch {
        Write-Error "GraphQL mutation failed: $_"
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
        
        # Extract repo name from various URL formats
        # GitHub: git@github.com:org/repo.git or https://github.com/org/repo.git
        if ($originUrl -match 'github\.com[:/](?<org>[^/]+)/(?<repo>[^\.]+)') {
            return "$($matches.org)/$($matches.repo)"
        }
        
        # Generic: user@host:path/repo.git
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
    
    $mutation = @"
mutation {
    createProject(input: { name: "$repoName", description: "Auto-initialized project" }) {
        project {
            id
        }
        errors
    }
}
"@
    
    $result = Invoke-GraphQLMutation -Mutation $mutation
    
    if ($result.data.createProject.errors) {
        Write-Error "Failed to create project: $($result.data.createProject.errors -join ', ')"
        exit 1
    }
    
    $projectId = $result.data.createProject.project.id
    Write-Host "Project created with ID: $projectId"
    
    $configPath = Join-Path $PSScriptRoot "opencode.json"
    $config = [ordered]@{
        '$schema' = "https://opencode.ai/config.json"
        mcp = @{
            devstack = @{
                type = "remote"
                url = "http://localhost:8087/mcp"
                enabled = "true"
            }
        }
    }

    $config | ConvertTo-Json -Depth 10 | Set-Content $configPath
    Write-Host "Created opencode.json"
    
    Write-Host "Initialization complete!"
}

function Get-CurrentProjectId {
    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Write-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    $query = @'
query GetProjects($first: Int) {
    getProjects(first: $first) {
        nodes {
            id
            name
        }
    }
}
'@

    $result = Invoke-GraphQLQuery -Query $query -Variables @{ first = 100 }

    if ($result.errors) {
        Write-Error "Failed to query projects: $($result.errors -join ', ')"
        exit 1
    }

    $project = $result.data.getProjects.nodes | Where-Object { $_.name -eq $repoName }
    if (-not $project) {
        Write-Error "No project matching '$repoName' found. Run init first."
        exit 1
    }

    return $project.id
}

function Plan-Defects {
    Write-Host "Running defects in Planning status for current project..."

    $projectId = Get-CurrentProjectId

    $query = @'
query GetDefects($projectId: UUID!) {
    getDefects(projectId: $projectId, first: 200) {
        nodes {
            id
            title
            status
            description
            acceptanceCriteria
            plan
        }
    }
}
'@

    $result = Invoke-GraphQLQuery -Query $query -Variables @{ projectId = $projectId }

    if ($result.errors) {
        Write-Error "Failed to query defects: $($result.errors -join ', ')"
        exit 1
    }

    $defects = $result.data.getDefects.nodes | Where-Object { $_.status -eq 'Planning' }

    if (-not $defects) {
        Write-Host "No defects in Planning status found for project."
        return
    }

    Write-Host "Found $($defects.Count) defect(s) in Planning status."

    foreach ($defect in $defects) {
        Write-Host "`nProcessing defect: $($defect.title) (ID: $($defect.id))"

        if ([string]::IsNullOrWhiteSpace($OpencodePrompt)) {
            $prompt = @"
Investigate the root cause for the failure. reproduce it, collect logs/traces and metrics, identify the failing component and code path
Propose a fix (if feasible within 5 minutes of research)
Use the update_defect tool to update the plan, securityImpact (if relevant), performanceImpact (if relevant), testPlan, deploymentPlan (if relevant), rootCause, openQuestions.
If there are no OpenQuestions, use the update_defect tool to change the state to Ready. If there are open questions, change the state to InReview.

Defect ID: $($defect.id)
Title: $($defect.title)
Status: $($defect.status)
Description: $($defect.description)
AcceptanceCriteria: $($defect.acceptanceCriteria)
Plan: $($defect.plan)
"@
        }
        else {
            $prompt = @"
$OpencodePrompt

Defect ID: $($defect.id)
Title: $($defect.title)
Status: $($defect.status)
Description: $($defect.description)
AcceptanceCriteria: $($defect.acceptanceCriteria)
Plan: $($defect.plan)
"@
        }

        Write-Host $prompt

        $Exe = "npx"
        $CommandArgs = @("opencode", "run", $prompt)

        & $Exe @CommandArgs

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Opencode returned non-zero exit code for defect $($defect.id)"
        }

        Start-Sleep -Seconds 2
    }

    Write-Host "`nAll defects processed."
}

function Run-Defects {
    Write-Host "Running defects in Planning status for current project..."

    $projectId = Get-CurrentProjectId

    $query = @'
query GetDefects($projectId: UUID!) {
    getDefects(projectId: $projectId, first: 200) {
        nodes {
            id
            title
            status
            description
            acceptanceCriteria
            plan
        }
    }
}
'@

    $result = Invoke-GraphQLQuery -Query $query -Variables @{ projectId = $projectId }

    if ($result.errors) {
        Write-Error "Failed to query defects: $($result.errors -join ', ')"
        exit 1
    }

    $defects = $result.data.getDefects.nodes | Where-Object { $_.status -eq 'Ready' }

    if (-not $defects) {
        Write-Host "No defects in Ready status found for project."
        return
    }

    Write-Host "Found $($defects.Count) defect(s) in Ready status."

    foreach ($defect in $defects) {
        Write-Host "`nProcessing defect: $($defect.title) (ID: $($defect.id))"

        if ([string]::IsNullOrWhiteSpace($OpencodePrompt)) {
            $prompt = @"
Create a fix for this issue.
Quality gates must pass.
Commit the changes.
Use the update_defect tool to change the state to Done. If the operation was not successful, change the status to InReview instead.

Defect ID: $($defect.id)
Title: $($defect.title)
Status: $($defect.status)
Description: $($defect.description)
AcceptanceCriteria: $($defect.acceptanceCriteria)
RootCause: $($defect.rootCause)
Plan: $($defect.plan)
"@
        }
        else {
            $prompt = @"
$OpencodePrompt

Defect ID: $($defect.id)
Title: $($defect.title)
Status: $($defect.status)
Description: $($defect.description)
AcceptanceCriteria: $($defect.acceptanceCriteria)
Plan: $($defect.plan)
"@
        }

        Write-Host $prompt

        $Exe = "npx"
        $CommandArgs = @("opencode", "run", $prompt)

        & $Exe @CommandArgs

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Opencode returned non-zero exit code for defect $($defect.id)"
        }

        Start-Sleep -Seconds 2
    }

    Write-Host "`nAll defects processed."
}


switch ($Command) {
    "init" {
        Initialize-Project
    }
    "run" {
        Plan-Defects
    }
}

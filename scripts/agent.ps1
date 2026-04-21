$ErrorActionPreference = 'Continue'

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$PromptsPath = Join-Path $PSScriptRoot "prompts"
$SpecsPath   = Join-Path $ProjectRoot "specs"
$AgentsFile  = Join-Path $PSScriptRoot "agents.md"
$DelaySeconds = 2

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

$GetFeaturesQuery = @'
query GetFeatures($projectId: UUID!, $first: Int!) {
  features(projectId: $projectId, first: $first) {
    nodes {
      id
      title
      status
      description
      acceptanceCriteria
      plan
      featureSubtype
      epicId
      type
      agentFeedback
      executionPlan
      securityImpact
      performanceImpact
      testPlan
      deploymentPlan
      blocking
    }
  }
}
'@

$GetTasksQuery = @'
query GetTasks($projectId: UUID!, $first: Int!) {
  tasks(projectId: $projectId, first: $first) {
    nodes {
      id
      title
      status
      deliverable
      acceptanceCriteria
      risks
      itemId
      projectId
      result
      errors
      commitHash
      complexityRating
      dependsOnDevTask
      promptTokens
      completionTokens
      executionDurationInSeconds
      model
      item {
        id
        title
        description
      }
    }
  }
}
'@

# API endpoint configuration
$ApiUrl = $env:DEVSTACK_API_URL
if ([string]::IsNullOrWhiteSpace($ApiUrl)) {
    $ApiUrl = "http://localhost:8087"
}

$GraphQLEndpoint = "$($ApiUrl.TrimEnd('/'))/graphql"

function Log-Info {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] INFO: $Message"
}

function Log-Error {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ERROR: $Message" -ForegroundColor Red
}

function Log-Phase {
    param([string]$Name)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  Phase: $Name" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Load-PromptFile {
    param([string]$FileName)

    $path = Join-Path $PromptsPath $FileName
    if (-not (Test-Path $path)) {
        Log-Error "Prompt file not found: $path"
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
    catch {
        Log-Error "GraphQL endpoint test failed: $Url - $_"
        return $false
    }
}

function Invoke-GraphQL {
    param([string]$Operation, [hashtable]$Variables = $null, [switch]$IsMutation)

    $body = @{ query = $Operation }
    if ($Variables) { $body.variables = $Variables }
    $jsonBody = $body | ConvertTo-Json -Depth 10

    try {
        Log-Info "Invoking GraphQL operation..."
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
        Log-Error "GraphQL $kind failed: $_"
        if ($responseBody) { Log-Error "Response body: $responseBody" }
        throw
    }
}

function Get-CurrentProjectId {
    Log-Info "Resolving current project ID..."

    try {
        $originUrl = git remote get-url origin 2>$null
        if ([string]::IsNullOrWhiteSpace($originUrl)) {
            Log-Error "No git remote 'origin' configured"
            return $null
        }
        $repoName = $null
        if ($originUrl -match 'github\.com[:/](?<org>[^/]+)/(?<repo>[^\.]+)') {
            $repoName = "$($matches.org)/$($matches.repo)"
        }
        elseif ($originUrl -match '/(?<repo>[^/]+?)(\.git)?$') {
            $repoName = $matches.repo
        }
        else {
            $repoName = $originUrl
        }
    }
    catch {
        Log-Error "Failed to get git remote origin: $_"
        return $null
    }

    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Log-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    Log-Info "Repository: $repoName"

    $result = Invoke-GraphQL -Operation $GetProjectsQuery -Variables @{ first = 100 }

    if ($result.errors) {
        Log-Error "Failed to query projects: $($result.errors -join ', ')"
        exit 1
    }

    $project = $result.data.projects.nodes | Where-Object { $_.name -eq $repoName }
    if (-not $project) {
        Log-Error "No project matching '$repoName' found. Run 'devstack.ps1 init' first."
        exit 1
    }

    Log-Info "Project ID: $($project.id)"
    return $project.id
}

function Run-OpencodePrompt {
    param([string]$Prompt)

    Log-Info "Running opencode prompt..."

    $npxArgs = @("opencode", "run", ($Prompt -replace "`r`n|`n|`r", " "))
    if (Test-Path $AgentsFile) { $npxArgs += @("--file", $AgentsFile) }

    Log-Info "Executing: npx @npxArgs"
    & npx @npxArgs

    if ($LASTEXITCODE -ne 0) {
        Log-Error "Opencode returned non-zero exit code"
        return $false
    }

    return $true
}

function Invoke-SpecAnalysisPhase {
    Log-Phase "Specification Analysis"

    Log-Info "Scanning specs directory: $SpecsPath"

    if (-not (Test-Path $SpecsPath)) {
        Log-Error "Specs directory not found: $SpecsPath"
        exit 1
    }

    $specFolders = Get-ChildItem -Path $SpecsPath -Directory
    $specFiles = Get-ChildItem -Path $SpecsPath -File -Filter "SPEC.md"

    if ($specFolders.Count -eq 0 -and $specFiles.Count -eq 0) {
        Log-Error "No specifications found under $SpecsPath"
        exit 1
    }

    $promptTemplate = Load-PromptFile "spec-analysis.prompt"

    # Build list of spec paths to analyze
    $specPaths = @()

    if ($specFiles.Count -gt 0) {
        # Root-level spec file (e.g., SPEC.md at specs/ level)
        $specPaths += "specs/**"
    }

    if ($specFolders.Count -gt 0) {
        # Each subfolder under specs/
        foreach ($folder in $specFolders) {
            $specPaths += "specs/$($folder.Name)/**"
        }
    }

    foreach ($specPath in $specPaths) {
        Log-Info "Analyzing specification: $specPath"

        $prompt = $promptTemplate -replace '\{SpecPath\}', $specPath
        Log-Info "Prompt: $prompt"

        $success = Run-OpencodePrompt -Prompt $prompt

        if (-not $success) {
            Log-Error "Spec analysis failed for: $specPath"
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    Log-Info "Specification analysis phase complete."
}

function Invoke-PlanningPhase {
    Log-Phase "Planning"

    $projectId = Get-CurrentProjectId
    if (-not $projectId) {
        Log-Error "Could not resolve project ID. Cannot proceed with planning."
        exit 1
    }

    $promptTemplate = Load-PromptFile "planning.prompt"

    # Fetch deliverables (features) in PLANNING status
    $result = Invoke-GraphQL -Operation $GetFeaturesQuery -Variables @{ projectId = $projectId; first = 100 }

    if ($result.errors) {
        Log-Error "Failed to query features: $($result.errors -join ', ')"
        exit 1
    }

    $deliverables = $result.data.features.nodes | Where-Object { $_.status -eq "PLANNING" }

    if (-not $deliverables) {
        Log-Info "No deliverables in PLANNING status found for project."
        return
    }

    Log-Info "Found $($deliverables.Count) deliverable(s) in PLANNING status."

    foreach ($deliverable in $deliverables) {
        Log-Info "Planning deliverable: $($deliverable.title) (ID: $($deliverable.id))"

        $prompt = $promptTemplate
        $prompt = $prompt -replace '\{\{Title\}\}', $deliverable.title
        $prompt = $prompt -replace '\{\{Description\}\}', $deliverable.description
        $prompt = $prompt -replace '\{\{DeliverableId\}\}', $deliverable.id

        Log-Info "Prompt: $prompt"

        $success = Run-OpencodePrompt -Prompt $prompt

        if (-not $success) {
            Log-Error "Planning failed for deliverable $($deliverable.id)"
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    Log-Info "Planning phase complete."
}

function Invoke-ExecutionPhase {
    Log-Phase "Execution"

    $projectId = Get-CurrentProjectId
    if (-not $projectId) {
        Log-Error "Could not resolve project ID. Cannot proceed with execution."
        exit 1
    }

    $promptTemplate = Load-PromptFile "execution.prompt"

    # Fetch AgentTasks (tasks) in READY status
    $result = Invoke-GraphQL -Operation $GetTasksQuery -Variables @{ projectId = $projectId; first = 100 }

    if ($result.errors) {
        Log-Error "Failed to query tasks: $($result.errors -join ', ')"
        exit 1
    }

    $tasks = $result.data.tasks.nodes | Where-Object { $_.status -eq "READY" }

    if (-not $tasks) {
        Log-Info "No AgentTasks in READY status found for project."
        return
    }

    Log-Info "Found $($tasks.Count) AgentTask(s) in READY status."

    foreach ($task in $tasks) {
        Log-Info "Executing AgentTask: $($task.title) (ID: $($task.id))"

        $prompt = $promptTemplate
        $prompt = $prompt -replace '\{\{Title\}\}', $task.title
        $prompt = $prompt -replace '\{\{Description\}\}', $task.description
        $prompt = $prompt -replace '\{\{AgentTaskId\}\}', $task.id

        Log-Info "Prompt: $prompt"

        $success = Run-OpencodePrompt -Prompt $prompt

        if (-not $success) {
            Log-Error "Execution failed for AgentTask $($task.id)"
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    Log-Info "Execution phase complete."
}

# Main execution
Log-Info "Starting Runner agent..."
Log-Info "GraphQL endpoint: $GraphQLEndpoint"

# Validate GraphQL endpoint
if (-not (Test-GraphQLEndpoint -Url $GraphQLEndpoint)) {
    Log-Error "Cannot connect to GraphQL endpoint at $GraphQLEndpoint"
    exit 1
}

Log-Info "GraphQL endpoint validated."

# Run phases
Invoke-SpecAnalysisPhase
Invoke-PlanningPhase
Invoke-ExecutionPhase

Log-Info "Runner agent complete."

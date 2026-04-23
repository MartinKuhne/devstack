param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("init", "run")]
    [string]$Command,

    [string]$ApiUrl = $env:DEVSTACK_API_URL
)

$ErrorActionPreference = 'Continue'

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$PromptsPath = Join-Path $PSScriptRoot "prompts"
$AgentsFile  = Join-Path $PSScriptRoot "agents.md"
$DelaySeconds = 5

if ([string]::IsNullOrWhiteSpace($ApiUrl)) {
    $ApiUrl = "http://localhost:8087"
}

# ── GraphQL Queries ──────────────────────────────────────────────────────────

$GetProjectQuery = @'
query GetProject($repoName: String) {
  projects(where: { name: { eq: $repoName } }) {
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

$GetDeliverablesQuery = @'
query GetDeliverables($first: Int!, $projectId: UUID!) {
  deliverables(first: $first, where: { projectId: { eq: $projectId } }) {
    nodes {
      id
      title
      status
      projectId
      description
      acceptanceCriteria
      executionPlan
      agentFeedback
      securityImpact
      performanceImpact
      testPlan
      deploymentPlan
      blocking
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$GetAgentTasksQuery = @'
query GetAgentTasks($first: Int!, $deliverableId: UUID!) {
  agentTasks(first: $first, where: { deliverableId: { eq: $deliverableId } }) {
    nodes {
      id
      title
      status
      deliverableId
      projectId
      description
      result
      errors
      commitHash
      complexityRating
      dependsOnAgentTaskId
      dependsOnAgentTask {
        id
        title
        status
      }
      promptTokens
      completionTokens
      executionDurationInSeconds
      agent
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$UpdateAgentTaskMutation = @'
mutation UpdateAgentTask($input: UpdateAgentTaskInput!) {
  updateAgentTask(input: $input) {
    id
    status
    result
    errors
    commitHash
  }
}
'@

$UpdateAgentTaskWithDurationMutation = @'
mutation UpdateAgentTaskWithDuration($input: UpdateAgentTaskInput!) {
  updateAgentTask(input: $input) {
    id
    status
    result
    errors
    commitHash
    executionDurationInSeconds
  }
}
'@

$TransitionAgentTaskStatusMutation = @'
mutation TransitionAgentTaskStatus($input: TransitionAgentTaskInput!) {
  transitionAgentTaskStatus(input: $input) {
    id
    status
  }
}
'@

$CheckAndMarkDeliverableDoneMutation = @'
mutation CheckAndMarkDeliverableDone($deliverableId: UUID!) {
  checkAndMarkDeliverableDone(deliverableId: $deliverableId)
}
'@

# ── Logging ──────────────────────────────────────────────────────────────────

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

# ── GraphQL ──────────────────────────────────────────────────────────────────

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

# ── Helpers ──────────────────────────────────────────────────────────────────

function Get-GitRemoteOrigin {
    try {
        $originUrl = git remote get-url origin 2>$null
        if ([string]::IsNullOrWhiteSpace($originUrl)) {
            Log-Error "No git remote 'origin' configured"
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
        Log-Error "Failed to get git remote origin: $_"
        return $null
    }
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

function Update-AgentTask {
    param(
        [string]$TaskId,
        [hashtable]$Fields,
        [switch]$WithDuration,
        [int]$ExecutionDurationSeconds
    )

    $updateVars = @{
        input = @{ id = $TaskId } + $Fields
    }

    if ($WithDuration) {
        $updateVars.input.executionDurationInSeconds = $ExecutionDurationSeconds
    }

    $mutation = if ($WithDuration) { $UpdateAgentTaskWithDurationMutation } else { $UpdateAgentTaskMutation }

    try {
        $result = Invoke-GraphQL -Operation $mutation -Variables $updateVars -IsMutation
        if ($result.errors) {
            Log-Error "Failed to update AgentTask $TaskId: $($result.errors -join ', ')"
            return $false
        }
        return $true
    }
    catch {
        Log-Error "Error updating AgentTask $TaskId: $_"
        return $false
    }
}

function Transition-AgentTaskStatus {
    param(
        [string]$TaskId,
        [string]$TargetStatus,
        [string]$Actor
    )

    $transitionVars = @{
        input = @{
            id = $TaskId
            targetStatus = $TargetStatus
            actor = $Actor
        }
    }

    try {
        $result = Invoke-GraphQL -Operation $TransitionAgentTaskStatusMutation -Variables $transitionVars -IsMutation
        if ($result.errors) {
            Log-Error "Failed to transition AgentTask $TaskId to $TargetStatus: $($result.errors -join ', ')"
            return $false
        }
        Log-Info "AgentTask $TaskId transitioned to $TargetStatus"
        return $true
    }
    catch {
        Log-Error "Error transitioning AgentTask $TaskId to $TargetStatus: $_"
        return $false
    }
}

function Get-CurrentProjectId {
    Log-Info "Resolving current project ID..."

    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Log-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    Log-Info "Repository: $repoName"

    $result = Invoke-GraphQL -Operation $GetProjectQuery -Variables @{ repoName = $repoName }

    $project = $result.data.projects.nodes | Where-Object { $_.name -eq $repoName }
    if (-not $project) {
        Log-Error "No project matching '$repoName' found. Run 'devstack.ps1 init' first."
        exit 1
    }

    Log-Info "Project ID: $($project.id)"
    return $project.id
}

function Get-LatestCommitHash {
    try {
        $hash = git log -1 --format=%H 2>$null
        if ($hash) {
            return $hash
        }
    }
    catch {
        Log-Error "Failed to get latest commit hash: $_"
    }
    return $null
}

function Run-OpencodePrompt {
    param([string]$Prompt)

    Log-Info "Running opencode prompt..."

    $npxArgs = @("opencode", "run", ($Prompt -replace "`r`n|`n|`r", " "))
    if (Test-Path $AgentsFile) { $npxArgs += @("--file", $AgentsFile) }

    Log-Info "Executing: npx opencode run <prompt>..."
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $output = & npx @npxArgs 2>&1
    $sw.Stop()

    if ($LASTEXITCODE -ne 0) {
        $errorOutput = $output -join "`n"
        Log-Error "Opencode returned non-zero exit code"
        return @{ Success = $false; ElapsedSeconds = [int]$sw.Elapsed.TotalSeconds; Output = $errorOutput }
    }

    return @{ Success = $true; ElapsedSeconds = [int]$sw.Elapsed.TotalSeconds; Output = $output -join "`n" }
}

# ── Phases ───────────────────────────────────────────────────────────────────

function Invoke-PlanningPhase {
    Log-Phase "Planning"

    $projectId = Get-CurrentProjectId

    $promptTemplate = Load-PromptFile "planning.prompt"

    Log-Info "Fetching deliverables for planning..."
    $result = Invoke-GraphQL -Operation $GetDeliverablesQuery -Variables @{ first = 100; projectId = $projectId }

    $deliverables = $result.data.deliverables.nodes | Where-Object { $_.status -eq "PLANNING" }

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
        $prompt = $prompt -replace '\{\{AcceptanceCriteria\}\}', $deliverable.acceptanceCriteria

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

    $promptTemplate = Load-PromptFile "execution.prompt"

    Log-Info "Fetching deliverables for execution..."
    $deliverablesResult = Invoke-GraphQL -Operation $GetDeliverablesQuery -Variables @{ first = 100; projectId = $projectId }

    if ($deliverablesResult.errors) {
        Log-Error "Failed to query deliverables: $($deliverablesResult.errors -join ', ')"
        exit 1
    }

    $allDeliverables = $deliverablesResult.data.deliverables.nodes
    if (-not $allDeliverables) {
        Log-Info "No deliverables found for project."
        return
    }

    Log-Info "Fetching AgentTasks in READY status..."
    $tasks = @()
    foreach ($deliverable in $allDeliverables) {
        $taskResult = Invoke-GraphQL -Operation $GetAgentTasksQuery -Variables @{ first = 100; deliverableId = $deliverable.id }
        if ($taskResult.errors) {
            Log-Error "Failed to query tasks for deliverable $($deliverable.id): $($taskResult.errors -join ', ')"
            continue
        }
        $deliverableTasks = $taskResult.data.agentTasks.nodes | Where-Object { $_.status -eq "READY" }
        if ($deliverableTasks) {
            $tasks += $deliverableTasks
        }
    }

    if (-not $tasks) {
        Log-Info "No AgentTasks in READY status found for project."
        return
    }

    Log-Info "Found $($tasks.Count) AgentTask(s) in READY status."

    foreach ($task in $tasks) {
        Log-Info "Executing AgentTask: $($task.title) (ID: $($task.id))"

        $prompt = $promptTemplate
        $prompt = $prompt -replace '\{\{Description\}\}', $task.description
        $prompt = $prompt -replace '\{\{AgentTaskId\}\}', $task.id

        Log-Info "Prompt: $prompt"

        $opencodeResult = Run-OpencodePrompt -Prompt $prompt
        $commitHash = Get-LatestCommitHash

        if ($opencodeResult.Success) {
            Update-AgentTask -TaskId $task.id.ToString() -Fields @{ result = $opencodeResult.Output; commitHash = $commitHash } -WithDuration -ExecutionDurationSeconds $opencodeResult.ElapsedSeconds
            Transition-AgentTaskStatus -TaskId $task.id.ToString() -TargetStatus "DONE" -Actor "runner-agent"
            Check-And-MarkDeliverableDone -DeliverableId $task.deliverableId
        }
        else {
            $errorOutput = if ($opencodeResult.Output) { $opencodeResult.Output } else { "Opencode returned non-zero exit code" }
            Update-AgentTask -TaskId $task.id.ToString() -Fields @{ errors = $errorOutput } -WithDuration -ExecutionDurationSeconds $opencodeResult.ElapsedSeconds
            Transition-AgentTaskStatus -TaskId $task.id.ToString() -TargetStatus "FAILED" -Actor "runner-agent"
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    Log-Info "Execution phase complete."
}

function Check-And-MarkDeliverableDone {
    param([string]$DeliverableId)

    Log-Info "Checking if all tasks for deliverable $($DeliverableId) are DONE via API..."

    try {
        $result = Invoke-GraphQL -Operation $CheckAndMarkDeliverableDoneMutation -Variables @{ deliverableId = $DeliverableId } -IsMutation

        $done = $result.data.checkAndMarkDeliverableDone
        if ($done) {
            Log-Info "Deliverable $($DeliverableId) marked as DONE."
        }
        else {
            Log-Info "Deliverable $($DeliverableId) not all tasks are DONE yet."
        }
        return $done
    }
    catch {
        Log-Error "Error checking deliverable $($DeliverableId): $_"
        return $false
    }
}

# ── Init ─────────────────────────────────────────────────────────────────────

function Initialize-Project {
    Write-Host "Initializing DevStack project..."

    $repoName = Get-GitRemoteOrigin
    if ([string]::IsNullOrWhiteSpace($repoName)) {
        Write-Error "Could not determine repository name. Please ensure git remote 'origin' is configured."
        exit 1
    }

    Write-Host "Repository: $repoName"

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
        $hasDevstackMcp = $mcpSection.PSObject.Properties['devstack'] -ne $null
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

    foreach ($perm in @('bash', 'question', 'external_directory')) {
        $permissionsSection[$perm] = 'deny'
    }

    $existingConfig | ConvertTo-Json -Depth 10 | Set-Content $opencodePath -Encoding UTF8
    Write-Host "Updated opencode.json"
    Write-Host "Initialization complete!"
}

# ── Runner ───────────────────────────────────────────────────────────────────

function Start-RunnerAgent {
    Log-Info "Starting Runner agent..."
    Log-Info "GraphQL endpoint: $GraphQLEndpoint"

    if (-not (Test-GraphQLEndpoint -Url $GraphQLEndpoint)) {
        Log-Error "Cannot connect to GraphQL endpoint at $GraphQLEndpoint"
        exit 1
    }

    Log-Info "GraphQL endpoint validated."

    while ($true) {
        Invoke-PlanningPhase
        Invoke-ExecutionPhase

        Log-Info "Waiting ${DelaySeconds} seconds before next loop..."
        Start-Sleep -Seconds $DelaySeconds
    }
}

# ── Entry Point ──────────────────────────────────────────────────────────────

switch ($Command) {
    "init"  { Initialize-Project }
    "run"   { Start-RunnerAgent }
}

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("init", "run")]
    [string]$Command,

    [string]$ApiUrl = $env:DEVSTACK_API_URL
)

$ErrorActionPreference = 'Continue'

$ProjectRoot = & git rev-parse --show-toplevel 2>$null
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
}
$PromptsPath = Join-Path $PSScriptRoot "prompts"
$AgentsFile  = Join-Path $ProjectRoot "AGENTS.md"
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
    id
    name
    description
    repository
  }
}
'@

$GetDeliverablesQuery = @'
query GetDeliverables($first: Int!, $projectId: UUID!) {
  deliverables(first: $first, where: { projectId: { eq: $projectId } }) {
    nodes {
      id
      title
      type
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

$GetDeliverablesByStatusQuery = @'
query GetDeliverablesByStatus($first: Int!, $projectId: UUID!, $status: DeliverableStatus!) {
  deliverables(first: $first, where: { projectId: { eq: $projectId }, status: { eq: $status } }) {
    nodes {
      id
      title
      type
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
      complexityRating
      dependsOnAgentTaskId
      dependsOnAgentTask {
        id
        status
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$GetAgentTasksByStatusQuery = @'
query GetAgentTasksByStatus($first: Int!, $deliverableId: UUID!, $status: AgentTaskStatus!) {
  agentTasks(first: $first, where: { deliverableId: { eq: $deliverableId }, status: { eq: $status } }) {
    nodes {
      id
      title
      status
      deliverableId
      projectId
      description
      complexityRating
      dependsOnAgentTaskId
      dependsOnAgentTask {
        id
        status
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$GetFailedAgentTasksQuery = @'
query GetFailedAgentTasks($first: Int!, $deliverableId: UUID!) {
  agentTasks(first: $first, where: { deliverableId: { eq: $deliverableId }, status: { in: [FAILED, REJECTED] } }) {
    nodes {
      id
      title
      status
      deliverableId
      projectId
      description
      complexityRating
      dependsOnAgentTaskId
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$GetDoneAgentTasksQuery = @'
query GetDoneAgentTasks($first: Int!, $deliverableId: UUID!) {
  agentTasks(first: $first, where: { deliverableId: { eq: $deliverableId }, status: { eq: DONE } }) {
    nodes {
      id
      title
      status
      deliverableId
      projectId
      description
      complexityRating
      dependsOnAgentTaskId
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$GetNeedsReviewAgentTasksQuery = @'
query GetNeedsReviewAgentTasks($first: Int!, $deliverableId: UUID!) {
  agentTasks(first: $first, where: { deliverableId: { eq: $deliverableId }, status: { eq: NEEDS_REVIEW } }) {
    nodes {
      id
      title
      status
      deliverableId
      projectId
      description
      complexityRating
      dependsOnAgentTaskId
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

$UpdateAgentTaskStatusMutation = @'
mutation UpdateAgentTaskStatus($id: UUID!, $targetStatus: AgentTaskStatus!) {
  updateAgentTaskStatus(id: $id, targetStatus: $targetStatus)
}
'@

$GetLargeLanguageModelsQuery = @'
query GetLargeLanguageModels($first: Int!) {
  largeLanguageModels(first: $first) {
    nodes {
      id
      url
      model
      modelAlias
      apiKey
      cost
      maxComplexity
      maxConcurrency
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
'@

$UpdateDeliverableStatusMutation = @'
mutation UpdateDeliverableStatus($id: UUID!, $targetStatus: DeliverableStatus!) {
  updateDeliverableStatus(id: $id, targetStatus: $targetStatus)
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

function Log-Warning {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] WARNING: $Message" -ForegroundColor Yellow
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
    param(
        [string]$Operation,
        [hashtable]$Variables = $null,
        [string]$OperationName = "",
        [switch]$IsMutation
    )

    $body = @{ query = $Operation }
    if ($Variables) { $body.variables = $Variables }
    if ($OperationName) { $body.operationName = $OperationName }
    $jsonBody = $body | ConvertTo-Json -Depth 10

    $kind = if ($IsMutation) { "mutation" } else { "query" }
    if ($OperationName) {
        Log-Info "Invoking GraphQL ${kind}: $OperationName"
    }
    else {
        Log-Info "Invoking GraphQL ${kind}..."
    }

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
        $result = Invoke-GraphQL -Operation $mutation -Variables $updateVars -IsMutation -OperationName "UpdateAgentTask"
        if ($result.errors) {
            Log-Error "Failed to update AgentTask ${TaskId}: $($result.errors -join ', ')"
            return $false
        }
        return $true
    }
    catch {
        Log-Error "Error updating AgentTask ${TaskId}: $_"
        return $false
    }
}

function Update-AgentTaskStatus {
    param(
        [string]$TaskId,
        [string]$TargetStatus
    )

    $statusVars = @{
        id = $TaskId
        targetStatus = $TargetStatus
    }

    try {
        $result = Invoke-GraphQL -Operation $UpdateAgentTaskStatusMutation -Variables $statusVars -IsMutation -OperationName "UpdateAgentTaskStatus"
        if ($result.errors) {
            Log-Error "Failed to update AgentTask ${TaskId} to ${TargetStatus}: $($result.errors -join ', ')"
            return $false
        }
        Log-Info "AgentTask $TaskId status updated to $TargetStatus"
        return $true
    }
    catch {
        Log-Error "Error updating AgentTask ${TaskId} to ${TargetStatus}: $_"
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

    $result = Invoke-GraphQL -Operation $GetProjectQuery -Variables @{ repoName = $repoName } -OperationName "GetProject"

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

function Get-LargeLanguageModels {
    Log-Info "Fetching LargeLanguageModel configurations..."

    try {
        $result = Invoke-GraphQL -Operation $GetLargeLanguageModelsQuery -Variables @{ first = 100 } -OperationName "GetLargeLanguageModels"
    }
    catch {
        $errMsg = $_.Exception.Message
        Log-Error "Failed to fetch LLM configurations: $errMsg"
        return @()
    }

    if ($result.errors) {
        $errDetails = $result.errors | ForEach-Object { if ($_.message) { $_.message } else { $_.toString() } }
        Log-Error "Failed to fetch LLM configurations: $($errDetails -join ', ')"
        return @()
    }

    $models = $result.data.largeLanguageModels.nodes
    Log-Info "Found $($models.Count) LLM configuration(s)"
    return $models
}

function Select-ModelForComplexity {
    param([int]$RequiredComplexity)

    $models = Get-LargeLanguageModels
    if (-not $models -or $models.Count -eq 0) {
        Log-Warning "No LLM configurations found, using default model"
        return $null
    }

    $eligibleModels = $models | Where-Object { $_.maxComplexity -ge $RequiredComplexity }
    if (-not $eligibleModels) {
        Log-Warning "No model supports complexity $RequiredComplexity, using lowest cost model"
        $eligibleModels = $models
    }

    $selectedModel = $eligibleModels | Sort-Object cost | Select-Object -First 1
    Log-Info "Selected model: $($selectedModel.model) (cost: $($selectedModel.cost), maxComplexity: $($selectedModel.maxComplexity))"
    return $selectedModel
}

function Sync-OpencodeProviders {
    param([string]$OpencodeConfigPath)

    $models = Get-LargeLanguageModels
    if (-not $models -or $models.Count -eq 0) {
        Log-Warning "No LLM configurations to sync"
        return
    }

    $existingJson = $null
    if (Test-Path $OpencodeConfigPath) {
        $existingJson = Get-Content $OpencodeConfigPath -Raw
    }

    $config = [ordered]@{}

    if ($existingJson) {
        $existing = $existingJson | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($existing) {
            foreach ($prop in $existing.PSObject.Properties) {
                if ($prop.Name -ne 'provider') {
                    $config[$prop.Name] = $prop.Value
                }
            }
        }
    }

    $providerSection = [ordered]@{}

    foreach ($model in $models) {
        $providerName = "devstack-$($model.id)"
        $modelKey = $model.model

        $options = [ordered]@{
            baseURL = $model.url
        }
        if ($model.apiKey) {
            $options['apiKey'] = $model.apiKey
        }

        $providerSection[$providerName] = [ordered]@{
            name    = $providerName
            npm     = '@ai-sdk/openai-compatible'
            options = $options
            models  = [ordered]@{
                $modelKey = [ordered]@{
                    name = $modelKey
                }
            }
        }
    }

    $config['provider'] = $providerSection

[System.IO.File]::WriteAllText($OpencodeConfigPath, ($config | ConvertTo-Json -Depth 10))
    Log-Info "Synced $($models.Count) LLM configurations to opencode.json"
}

function Run-OpencodePrompt {
    param(
        [string]$Prompt,
        [string]$PromptName,
        [int]$RequiredComplexity = 0
    )

    Log-Info "Running opencode prompt: $PromptName"

    $model = $null
    if ($RequiredComplexity -gt 0) {
        $model = Select-ModelForComplexity -RequiredComplexity $RequiredComplexity
    }

    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        [System.IO.File]::WriteAllText($tempFile, $Prompt, [System.Text.Encoding]::UTF8)

        $npxArgs = @("opencode", "run", "Execute the commands:", "--file", $tempFile)
        if (Test-Path $AgentsFile) { $npxArgs += @("--file", $AgentsFile) }
        if ($model) { $npxArgs += @("--model", "devstack-$($model.id)/$($model.model)") }

        Log-Info "Executing: npx opencode run `"Execute the commands:`" --file $tempFile..."
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $output = & npx @npxArgs 2>&1
        $sw.Stop()

        $outputText = $output -join "`n"
        if ($outputText) {
            Log-Info "Opencode output:`n$outputText"
        }

        if ($LASTEXITCODE -ne 0) {
            Log-Error "Opencode returned non-zero exit code"
            return @{ Success = $false; ElapsedSeconds = [int]$sw.Elapsed.TotalSeconds; Output = $outputText }
        }

        return @{ Success = $true; ElapsedSeconds = [int]$sw.Elapsed.TotalSeconds; Output = $outputText }
    }
    finally {
        if (Test-Path $tempFile) { Remove-Item $tempFile -Force }
    }
}

# ── Phases ───────────────────────────────────────────────────────────────────

function Invoke-DeliverableStateTransitions {
    Log-Phase "Deliverable State Transitions"

    $projectId = Get-CurrentProjectId

    $statusesToProcess = @("DESIGN", "PLAN", "MERGE")

    foreach ($status in $statusesToProcess) {
        try {
            $result = Invoke-GraphQL -Operation $GetDeliverablesByStatusQuery -Variables @{ first = 1; projectId = $projectId; status = $status } -OperationName "GetDeliverablesByStatus"
        }
        catch {
            $errMsg = $_.Exception.Message
            Log-Error "Failed to fetch deliverables with status $status`: $errMsg"
            continue
        }

        if ($result.errors) {
            $errDetails = $result.errors | ForEach-Object { if ($_.message) { $_.message } else { $_.toString() } }
            Log-Error "Failed to fetch deliverables with status $status`: $($errDetails -join ', ')"
            continue
        }

        $deliverables = $result.data.deliverables.nodes
        if (-not $deliverables) {
            continue
        }

        foreach ($deliverable in $deliverables) {
            $processed = $false

            if ($deliverable.status -eq "DESIGN") {
                if ($deliverable.type -eq "SPIKE") {
                    $promptTemplate = Load-PromptFile "research.prompt"
                    $complexity = 10
                    $promptName = "research"
                    $processed = $true
                }
                elseif ($deliverable.type -eq "FEATURE") {
                    $promptTemplate = Load-PromptFile "design.prompt"
                    $complexity = 10
                    $promptName = "design"
                    $processed = $true
                }
            }
            elseif ($deliverable.status -eq "PLAN") {
                if ($deliverable.type -eq "DEFECT") {
                    $promptTemplate = Load-PromptFile "root-cause.prompt"
                    $complexity = 8
                    $promptName = "root-cause"
                    $processed = $true
                }
                elseif ($deliverable.type -eq "FEATURE" -or $deliverable.type -eq "MAINTENANCE") {
                    $promptTemplate = Load-PromptFile "plan.prompt"
                    $complexity = 8
                    $promptName = "plan"
                    $processed = $true
                }
            }
            elseif ($deliverable.status -eq "MERGE") {
                $promptTemplate = Load-PromptFile "pr.prompt"
                $complexity = 8
                $promptName = "merge"
                $processed = $true
            }

            if ($processed) {
                Log-Info "Processing deliverable: $($deliverable.title) (ID: $($deliverable.id)) type: $($deliverable.type) status: $($deliverable.status)"

                $prompt = $promptTemplate
                $prompt = $prompt -replace '\{\{Title\}\}', $deliverable.title
                $prompt = $prompt -replace '\{\{Description\}\}', $deliverable.description
                $prompt = $prompt -replace '\{\{AcceptanceCriteria\}\}', $deliverable.acceptanceCriteria
                $prompt = $prompt -replace '\{\{DeliverableId\}\}', $deliverable.id

                $opencodeResult = Run-OpencodePrompt -Prompt $prompt -PromptName $promptName -RequiredComplexity $complexity

                if (-not $opencodeResult.Success) {
                    Log-Error "Prompt failed for deliverable $($deliverable.id)"
                }

                Start-Sleep -Seconds $DelaySeconds
            }
        }
    }

    Log-Info "Deliverable state transitions complete."
}

function Invoke-ExecutionPhase {
    Log-Phase "Execution"

    $projectId = Get-CurrentProjectId

    $promptTemplate = Load-PromptFile "implement.prompt"

    Log-Info "Fetching deliverables in IMPLEMENT status..."
    $deliverablesResult = Invoke-GraphQL -Operation $GetDeliverablesByStatusQuery -Variables @{ first = 1; projectId = $projectId; status = "IMPLEMENT" } -OperationName "GetDeliverablesByStatus"

    if ($deliverablesResult.errors) {
        Log-Error "Failed to query deliverables: $($deliverablesResult.errors -join ', ')"
        return
    }

    $allDeliverables = $deliverablesResult.data.deliverables.nodes
    if (-not $allDeliverables) {
        Log-Info "No deliverables in IMPLEMENT status found for project."
        return
    }

    Log-Info "Fetching AgentTasks in READY status..."
    $tasks = @()
    $deliverableTaskMap = @{}
    foreach ($deliverable in $allDeliverables) {
        $taskResult = Invoke-GraphQL -Operation $GetAgentTasksByStatusQuery -Variables @{ first = 100; deliverableId = $deliverable.id; status = "READY" } -OperationName "GetAgentTasksByStatus"
        if ($taskResult.errors) {
            Log-Error "Failed to query tasks for deliverable $($deliverable.id): $($taskResult.errors -join ', ')"
            continue
        }
        $deliverableTasks = $taskResult.data.agentTasks.nodes
        if ($deliverableTasks) {
            foreach ($t in $deliverableTasks) {
                $tasks += $t
                $deliverableTaskMap[$t.id] = $deliverable
            }
        }
    }

    if (-not $tasks) {
        Log-Info "No AgentTasks in READY status found for project."
        return
    }

    Log-Info "Found $($tasks.Count) AgentTask(s) in READY status."

    foreach ($task in $tasks) {
        Log-Info "Executing AgentTask: $($task.title) (ID: $($task.id)) complexity: $($task.complexityRating)"

        $prompt = $promptTemplate
        $prompt = $prompt -replace '\{\{Description\}\}', $task.description
        $prompt = $prompt -replace '\{\{AgentTaskId\}\}', $task.id

        $opencodeResult = Run-OpencodePrompt -Prompt $prompt -PromptName "implement" -RequiredComplexity $task.complexityRating
        $commitHash = Get-LatestCommitHash

        if ($opencodeResult.Success) {
            Update-AgentTask -TaskId $task.id.ToString() -Fields @{ result = $opencodeResult.Output; commitHash = $commitHash } -WithDuration -ExecutionDurationSeconds $opencodeResult.ElapsedSeconds
            Update-AgentTaskStatus -TaskId $task.id.ToString() -TargetStatus "DONE"
        }
        else {
            $errorOutput = if ($opencodeResult.Output) { $opencodeResult.Output } else { "Opencode returned non-zero exit code" }
            Update-AgentTask -TaskId $task.id.ToString() -Fields @{ errors = $errorOutput } -WithDuration -ExecutionDurationSeconds $opencodeResult.ElapsedSeconds
            Update-AgentTaskStatus -TaskId $task.id.ToString() -TargetStatus "FAILED"
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    foreach ($deliverable in $allDeliverables) {
        $failedResult = Invoke-GraphQL -Operation $GetFailedAgentTasksQuery -Variables @{ first = 100; deliverableId = $deliverable.id } -OperationName "GetFailedAgentTasks"
        if ($failedResult.errors) { continue }

        $needsReviewResult = Invoke-GraphQL -Operation $GetNeedsReviewAgentTasksQuery -Variables @{ first = 100; deliverableId = $deliverable.id } -OperationName "GetNeedsReviewAgentTasks"
        if ($needsReviewResult.errors) { continue }

        $failedTask = $failedResult.data.agentTasks.nodes | Select-Object -First 1
        $needsReviewTask = $needsReviewResult.data.agentTasks.nodes | Select-Object -First 1

        if (($failedTask -or $needsReviewTask) -and $deliverable.status -ne "FAILED") {
            Log-Info "Deliverable $($deliverable.id) has task in problematic state, setting to FAILED"
            $transitionVars = @{
                id = $deliverable.id
                targetStatus = "FAILED"
            }
            $transitionResult = Invoke-GraphQL -Operation $UpdateDeliverableStatusMutation -Variables $transitionVars -IsMutation -OperationName "UpdateDeliverableStatus"
        }

        $doneResult = Invoke-GraphQL -Operation $GetDoneAgentTasksQuery -Variables @{ first = 100; deliverableId = $deliverable.id } -OperationName "GetDoneAgentTasks"
        if ($doneResult.errors) { continue }

        $doneTasks = $doneResult.data.agentTasks.nodes
        $allDone = $doneTasks -and $doneTasks.Count -gt 0
        
        if ($allDone -and $deliverable.status -ne "DONE" -and $deliverable.status -ne "NEEDS_REVIEW") {
            Log-Info "All tasks for deliverable $($deliverable.id) are DONE, running pr.prompt"

            $prPromptTemplate = Load-PromptFile "pr.prompt"
            $prPrompt = $prPromptTemplate
            $prPrompt = $prPrompt -replace '\{\{Title\}\}', $deliverable.title
            $prPrompt = $prPrompt -replace '\{\{DeliverableId\}\}', $deliverable.id

            $prResult = Run-OpencodePrompt -Prompt $prPrompt -PromptName "pull-request" -RequiredComplexity 4

            if ($prResult.Success) {
                Log-Info "PR prompt completed for deliverable $($deliverable.id)"
            }
            else {
                Log-Error "PR prompt failed for deliverable $($deliverable.id)"
            }
        }
    }

    Log-Info "Execution phase complete."
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

    $result = Invoke-GraphQL -Operation $GetProjectQuery -Variables @{ repoName = $repoName } -OperationName "GetProject"

    $existingProject = $result.data.projects.nodes | Where-Object { $_.name -eq $repoName }

    if ($existingProject) {
        Write-Host "Project '$repoName' already exists with ID: $($existingProject.id)"
    }
    else {
        $result = Invoke-GraphQL -Operation $CreateProjectMutation -Variables @{ input = @{ name = $repoName; description = 'Auto-initialized project'; repository = $repoName } } -IsMutation -OperationName "CreateProject"

        if ($result.errors) {
            $errorMessages = $result.errors | ForEach-Object { $_.message }
            Write-Error "Failed to create project: $($errorMessages -join ', ')"
            exit 1
        }

        $existingProject = $result.data.createProject
        Write-Host "Project created with ID: $($existingProject.id)"
    }

    $opencodePath = Join-Path (Split-Path $PSScriptRoot -Parent) "opencode.json"

    $config = [ordered]@{
        '$schema' = "https://opencode.ai/config.json"
        mcp       = [ordered]@{
            devstack = [ordered]@{
                type    = "remote"
                url     = "http://localhost:8088/mcp"
                enabled = $true
            }
        }
    }

    if (Test-Path $opencodePath) {
        $existing = Get-Content $opencodePath -Raw | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($existing) {
            foreach ($prop in $existing.PSObject.Properties) {
                if ($prop.Name -notin @('$schema', 'mcp', 'provider')) {
                    $config[$prop.Name] = $prop.Value
                }
            }
            if ($existing.mcp) {
                foreach ($prop in $existing.mcp.PSObject.Properties) {
                    if ($prop.Name -ne 'devstack') {
                        $config['mcp'][$prop.Name] = $prop.Value
                    }
                }
            }
        }
    }

    [System.IO.File]::WriteAllText($opencodePath, ($config | ConvertTo-Json -Depth 10))
    Write-Host "Updated opencode.json"

    Sync-OpencodeProviders -OpencodeConfigPath $opencodePath

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

    $opencodePath = Join-Path (Split-Path $PSScriptRoot -Parent) "opencode.json"
    Sync-OpencodeProviders -OpencodeConfigPath $opencodePath

    Log-Info "GraphQL endpoint validated."

    while ($true) {
        Invoke-DeliverableStateTransitions
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

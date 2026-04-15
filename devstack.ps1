param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("init", "run")]
    [string]$Command
)

$ErrorActionPreference = 'Continue'

$ApiUrl = "http://localhost:5000"
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
    
    $opencodeDir = Join-Path $PSScriptRoot ".opencode"
    if (-not (Test-Path $opencodeDir)) {
        New-Item -ItemType Directory -Force -Path $opencodeDir | Out-Null
    }
    
    $configPath = Join-Path $opencodeDir "config.json"
    $config = @{
        mcpServers = @{
            devstack = @{
                command = "npx"
                args = @("-y", "@modelcontextprotocol/server-stdio", "http://localhost:5000/mcp")
            }
        }
    }
    
    $config | ConvertTo-Json -Depth 10 | Set-Content $configPath
    Write-Host "Created .opencode/config.json"
    
    Write-Host "Initialization complete!"
}

function Run-Tasks {
    Write-Host "Running pending tasks..."
    
    $query = @"
{
    tasks(first: 50) {
        nodes {
            id
            title
            status
        }
    }
}
"@
    
    $result = Invoke-GraphQLQuery -Query $query
    
    if ($result.errors) {
        Write-Error "Failed to query tasks: $($result.errors -join ', ')"
        exit 1
    }
    
    $tasks = $result.data.tasks.nodes
    $todoTasks = $tasks | Where-Object { $_.status -in @('TODO', 'IN_PROGRESS') }
    
    if (-not $todoTasks) {
        Write-Host "No pending tasks found."
        return
    }
    
    Write-Host "Found $($todoTasks.Count) pending tasks"
    
    foreach ($task in $todoTasks) {
        Write-Host "`nProcessing task: $($task.title) (ID: $($task.id))"
        
        $taskQuery = @"
query GetTaskById($id: ID!) {
    task(id: $id) {
        id
        title
        description
        status
    }
}
"@
        
        $taskResult = Invoke-GraphQLQuery -Query $taskQuery -Variables @{ id = $task.id }
        
        if ($taskResult.errors) {
            Write-Warning "Failed to get task details for $($task.id): $($taskResult.errors -join ', ')"
            continue
        }
        
        $taskDetails = $taskResult.data.task
        $prompt = @"
Task ID: $($taskDetails.id)
Title: $($taskDetails.title)
Description: $($taskDetails.description)
Status: $($taskDetails.status)

Please complete this task and mark it as done when finished.
"@
        
        Write-Host $prompt
        
        # Run opencode with the task prompt
        $Exe = "npx"
        $CommandArgs = @("opencode", "run", $prompt, "--file", "agents.md", "--file", "docs\TOOLS.md")
        
        & $Exe @CommandArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Opencode returned non-zero exit code for task $($task.id)"
        }
        
        Start-Sleep -Seconds 2
    }
    
    Write-Host "`nAll tasks processed."
}

switch ($Command) {
    "init" {
        Initialize-Project
    }
    "run" {
        Run-Tasks
    }
}

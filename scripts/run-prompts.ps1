$AgentsFile = Join-Path $PSScriptRoot "..\agents.md"
Write-Host $AgentsFile
$DelaySeconds = 2

$Prompts = @(
    @"
Compare the specification under ```specs/graphql/**``` with the actual implementation 
and create todos to change the implementation to match the specification.
Execute the todos.
The spec is the source of truth. Commit the changes once quality gates pass.
"@,

    @"
Compare the specification under ```specs/mcp/**``` with the actual implementation 
and create todos to change the implementation to match the specification.
Execute the todos.
The spec is the source of truth. Commit the changes once quality gates pass.
"@,

@"
Compare the specification under ```specs/adminui/**``` with the actual implementation 
and create todos to change the implementation to match the specification.
Execute the todos.
The spec is the source of truth. Commit the changes once quality gates pass.
"@,

@"
Compare the specification under ```specs/runner/**``` with the actual implementation
 and create todos to change the implementation to match the specification.
Execute the todos.
The spec is the source of truth. Commit the changes once quality gates pass.
"@

)

$Iterations = 10

for ($i = 1; $i -le $Iterations; $i++) {
    Write-Host "`n=== Iteration $i of $Iterations ==="

    foreach ($prompt in $Prompts) {
        Write-Host "`n--- Running prompt ---"
        Write-Host $prompt

        $npxArgs = @("opencode", "run", ($prompt -replace "`r`n|`n|`r", " "), "--file", $AgentsFile)
#       if (Test-Path $AgentsFile) { $npxArgs += @("--file", $AgentsFile) }
        Write-Host $npxArgs
        & npx @npxArgs

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Opencode returned non-zero exit code for prompt: $($prompt.Substring(0, [Math]::Min(80, $prompt.Length)))..."
        }

        if ($DelaySeconds -gt 0 -and $prompt -ne $Prompts[-1]) {
            Start-Sleep -Seconds $DelaySeconds
        }
    }

    Write-Host "`nIteration $i complete."
}

Write-Host "`nAll iterations complete."

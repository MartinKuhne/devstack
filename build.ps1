$ErrorActionPreference = 'Continue'

$BasePrompt = "Use he saga_task_list tool to find and finish any in progress items. If there are no in progress items, Use he saga_task_list tool to find the next task. When complete, code quality criteria are met and quality gates pass, commit the changes. Then mark the task as complete.Do not wait for approvals. Do not execute more tasks. Make the best decisions you can to proceed with the implementation."

$Exe = "npx"
$CommandArgs = @("opencode", "run", $BasePrompt, "--file", "agents.md", "--file", "docs\TOOLS.md")

$LogDir = Join-Path $PSScriptRoot "logs"
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

while ($true) {
    $ts = Get-Date -Format "yyyyMMdd_HHmmss"
    $logFile = Join-Path $LogDir "run-$ts.log"
    Write-Host "Starting command at $ts; logging to $logFile"

    # Run the command, capture stdout to log and show on console; stderr goes directly to console
    & $Exe @CommandArgs | Tee-Object -FilePath $logFile

    # Capture exit code of the last process
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        $endTs = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Write-Host "$endTs Command exited with code $exitCode. Stopping loop."
#        break
    }

    Write-Host "Restarting..."
    #Start-Sleep -Seconds 2
}


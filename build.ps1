$ErrorActionPreference = 'Continue'

$BasePrompt = "Use he saga_task_list tool to find and finish any in progress items. If there are no in progres items, Use he saga_task_list tool to find the next task. When complete and quality gates pass, mark as completed. commit the changes. Do not wait for approvals. Do not execute more tasks. Skip tasks that are unclear or have open questions and create a docs/OPEN_QUESTIONS.md file with these questions."

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


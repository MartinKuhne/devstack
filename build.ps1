$ErrorActionPreference = 'Continue'

$Prompt = "Index the codebase, read ./agents.md then Execute the next unblocked task from the saga tool. when tests pass, mark as completed. commit the changes. Do not wawit for approvals. Skip items that have open questions and create a markdown file with these questions. Focus on progress over perfection."

$Exe = "npx"
$CommandArgs = @("opencode", "run", $Prompt)

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


Before producing final PowerShell code, validate it against these quality gates:

1. Script runs without errors on PowerShell 7.4 under Set-StrictMode -Version Latest.
3. All external operations use structured try/catch with actionable errors.
4. Functions return structured objects with no unintended output.
5. Code is idempotent and avoids global side effects.
6. No secrets or credentials appear in code or logs.
8. Naming follows Verb-Noun and PowerShell style conventions.

```scripts\devstack.ps1 init``` runs without errors

Reject any solution that fails any gate.

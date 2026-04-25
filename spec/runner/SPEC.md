<!--
FILE IS IMMUTABLE

This file is part of the system specification.
Automated agents MUST NOT:
- rewrite
- reformat
- optimize
- refactor
- regenerate
- insert or remove content

Only human maintainers may update this file.
-->

# Runner Specification

# Goals

Run a plan, execute, review loop using an AI agent

# Components

- A powershell script ```scripts/devstack.ps1```
- A set of graphql queries used by the script, contained in the script as strings
- A set of prompts used by the script, stored as files in the ./scripts/prompts folder

# Capabilities

Execute prompts with OpenCode

```
    $npxArgs = @("opencode", "run", ($prompt -replace "`r`n|`n|`r", " "))
    if (Test-Path $AgentsFile) { $npxArgs += @("--file", $AgentsFile) }
    & npx @npxArgs
```
# Functional Requirements (EARS Format)

## Ubiquitous Requirements
- [REQ-AG-001] The system shall determine the current repository upon startup using the github repository name (if available)
- [REQ-AG-002] The system shall log all prompts and program invocations to the console
- [REQ-AG-003] The system shall accept GUIDs in all legal formats: https://learn.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=net-10.0

## Event-Driven Requirements  
- [REQ-AG-100] When there is no project matching the current repository in GraphQL, the system shall create it
- [REQ-AG-101] When the ```opencode.json``` file in the repository root does not contain an entry for the DevStack MCP server, the system shall add it
- [REQ-AG-102] When the system starts up, it shall update the opencode configuration at the repository root to [deny] the [bash], [question] and [external_directory] permissions.
- [REQ-AG-103] When an AgentTask execution completes, the system shall update the ExecutionDurationInSeconds of the AgentTask with the time it took to run OpenCode
- [REQ-AG-100] Before the first AgentTask executes, the system shall check out main in git ```git checkout main```, then create a feature branch of the name agent/AgentTaskId. Example: ```git checkout -b agent/be7b213c-6e30-4cef-a679-21a39ade7db9```
- [REQ-AG-101] When the last AgentTask has executed and all the AgentTasks are in the DONE state, execute the [Pull Request Phase]
- [REQ-AG-102] When the last AgentTask has executed and all the AgentTasks are not in the DONE state, change the Deliverable to FAILED

## State-Driven Requirements
- [REQ-AG-200] While there are Deliverables for the Project in Status = PLANNING, the system shall execute the Planning Phase (below)
- [REQ-AG-201] While there are AgentTasks for the Project in Status = READY, the system shall execute the Execution Phase (below)

## Unwanted Behavior Requirements
- [REQ-AG-300] When an ```opencode.json``` file is present, the system shall not delete or overwrite the file or delete existing content from it

## Opencode configuration example

```
{
    "$schema": "https://opencode.ai/config.json",
    "mcp": {
        "devstack": {
            "type": "remote",
            "url": "http://localhost:8088/mcp",
            "enabled": true
        }
    }
}
```

## Planning phase

Invoke OpenCode with ./scripts/prompts/planning.prompt

Substitute {{Title}}, {{Description}}, {{AcceptanceCriteria}}, {{DeliverableId}} with the fields of the same name from the Deliverable

## Execution phase

Invoke OpenCode with ./scripts/prompts/execution.prompt

Substitute {{Description}}, {{AgentTaskId}} with the fields of the same name from the Deliverable

## Bug fix phase

Invoke OpenCode with ./scripts/prompts/fix.prompt

Substitute {{Description}}, {{AgentTaskId}} with the fields of the same name from the Deliverable

## Pull request phase

Invoke OpenCode with ./scripts/prompts/pr.prompt

Substitute {{Title}}, {{DeliverableId}} with the fields of the same name from the Deliverable

# Technical specification

- [OpenCode permissions](https://opencode.ai/docs/permissions)
- [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/lang-spec/chapter-15?view=powershell-7.6)
- [Powershell guidelines](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines?view=powershell-7.6)

# Quality gates

```Get-Command -syntax '.\scripts\devstack.ps1'```

Skip any other tests as this is a script

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
- A set of prompts used by the script, contained in the script as strings

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

## Event-Driven Requirements  
- [REQ-AG-100] When there is no project matching the current repository in GraphQL, the system shall create it
- [REQ-AG-101] When the ```opencode.json``` file in the repository root does not contain an entry for the DevStack MCP server, the system shall add it

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

Invoke OpenCode with the following prompt:

"Plan the implementation of the Deliverable. Do not make any changes to the project
{{Description}}
Acceptance Criteria: {{AcceptanceCriteria}}
DeliverableId: {{DeliverableId}}
Update the Deliverable's ExecutionPlan, SecurityImpact, PerformanceImpact, TestPlan, DeploymentPlan
If architecture and/or technology choices are ambigous, change the Deliverable status to NeedsReview, then update the Deliverable's Blocking field and STOP.
Break the Plan down into Steps that can be completed by an AI agent in less than 20 minutes. Create AgentTask objects with the devstack tool for the steps. Change the Deliverable status to READY"

Substitute {{Title}}, {{Description}}, {{AcceptanceCriteria}}, {{DeliverableId}} with the fields of the same name from the Deliverable

## Execution phase

Invoke OpenCode with the following prompt:

"Implement the change
{{Description}}
AgentTaskId: {{AgentTaskId}}
If successful, update the Result, CommitHash fields and change the AgentTask Status to Done.
Commit the changes with a detailed description of what has been changed.
If not successful, update the Result and Errors fields and change the AgentTask Status to Failed"

Substitute {{Description}}, {{AgentTaskId}} with the fields of the same name from the Deliverable

# Technical specification

- [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/lang-spec/chapter-15?view=powershell-7.6)
- [Powershell guidelines](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines?view=powershell-7.6)

# Quality gates

```Get-Command -syntax '.\scripts\devstack.ps1'```

Skip any other tests as this is a script

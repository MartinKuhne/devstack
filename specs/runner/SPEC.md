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
- REQ-AG-001: The system shall determine the current repository upon startup using the github repository name (if available)

## Event-Driven Requirements  
- REQ-AG-002: When there is no project matching the current repository in GraphQL, the system shall create it
- REQ-AG-003: When the ```opencode.json``` file in the repository root does not contain an entry for the DevStack MCP server, the system shall add it

## State-Driven Requirements
- REQ-AG-003: While there are Deliverables for the Project in Status = PLANNING, the system shall execute the Planning Phase (below)
- REQ-AG-004: While there are Deliverables for the Project in Status = READY, the system shall execute the Execution Phase (below)

## Unwanted Behavior Requirements
- REQ-AG-005: Dot not delete or overwrite or delete existing content from ```opencode.json```

# Operation

- Retrieve the Project ID

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

- Query the 
- For each Deliverable, execute the Planning phase (below)
- Query the 
- For each Deliverable, execute the Execution phase (below)

## Planning phase

Prompt: Plan the implementation of the Deliverable. Do not make any changes to the project
{{Title}}
{{Description}}
DeliverableId: {{DeliverableId}}
Update the deliverable with the Plan and update all the fields with the findings. 
If architecture and/or technology choices are ambigous, do not create AgentTasks, change the Deliverable status to NeedsReview.
Otherwise, Create AgentTasks that can be completed by an AI agent in less than 60 minutes. Indicate the complexity of the task. 

## Execution phase

Prompt: Implement the change
{{Title}}
{{Description}}
AgentTaskId: {{AgentTaskId}}
If successful, change the AgentTask Status to Done
It not successful, change the AgentTask Status to NeedsReview

# Technical specification

- [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/lang-spec/chapter-15?view=powershell-7.6)
- [Powershell guidelines](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines?view=powershell-7.6)


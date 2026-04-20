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

# Operation

- Determine the current project from the github repository name
- Create a project if it does not exist
- Add the MCP server to opencode.json in the repository root, like so
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
Edit but do not overwrite the file

- Query the Deliverables for the Project in Status = PLANNING
- For each Deliverable, execute the Planning phase (below)
- Query the Deliverables for the Project in Status = READY
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


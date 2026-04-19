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

- A powershell script ```scripts/agent.ps1```
- A set of graphql queries used by the script in ```scripts/queries```
- A set of prompts used by the script in ```scripts/prompts```

# Capabilities

Run a loop executing the following prompts with opencode. Sample code:

```
    $npxArgs = @("opencode", "run", ($prompt -replace "`r`n|`n|`r", " "))
    if (Test-Path $AgentsFile) { $npxArgs += @("--file", $AgentsFile) }
    & npx @npxArgs
```

## Specification analysis phase

Prompt: Compare the specification under ```specs/**``` with the actual implementation and create Deliverables and AgentTasks to change the implementation to match the specification. Create Deliverables and AgentTaks in the Ready state when the architecture and technoloy choices
are unambiguous. Create just a Deliverable in state NeedsReview if there are open questions or architecture and/or technology choices are ambigous.
The spec is the source of truth and the code must be changed accordingly.

If there are multiple folders under ```specs/**```, instead run the prompt once for the contents of each folder. For example, the prompt would start with: Compare the specification under ```specs/graphql/**```

## Planning phase

Prompt: Plan the implementation of Deliverables in the Planning state. Create AgentTasks that can be completed by an AI agent in less than 20 minutes. Indicate the complexity of the task. If architecture and/or technology choices are ambigous, do not create AgentTasks, instead change the Deliverable status to NeedsReview.

Append all Deliverable fields to the prompt

## Execution phase

Prompt: Retrieve AgentTasks in Ready status for the current project and execute the tasks. When the task is complete and quality gates pass, commit the changes and change the AgentTask status to Done. If there were any errors, update the AgentTask with that information and move it to the Failed status.

Append all AgentTask fields to the prompt

# Technical specification

- [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/lang-spec/chapter-15?view=powershell-7.6)
- [Powershell guidelines](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines?view=powershell-7.6)


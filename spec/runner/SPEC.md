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
- [REQ-AG-002] The system shall log prompts names and program invocations to the console
- [REQ-AG-003] The system shall accept GUIDs in all legal formats: https://learn.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=net-10.0
- [REQ-AG-004] The system shall make state changes using the graphql API and schema at $(RepositoryRoot)/src/Server/DevStack.Api/GraphQL/schema.graphql
- [REQ-AG-005] The system shall keep the opencode providers in sync with the list of [LargeLanguageModel] in GraphQL. The provider name shall be 'devstack-(id)' where (id) is the [LargeLanguageModel][Id].

## Event-Driven Requirements  
- [REQ-AG-100] When there is no project matching the current repository in GraphQL, the system shall create it
- [REQ-AG-101] When the ```opencode.json``` file in the repository root does not contain an entry for the DevStack MCP server, the system shall add it
- [REQ-AG-102] When an AgentTask execution completes, the system shall update the ExecutionDurationInSeconds of the AgentTask with the time it took to run OpenCode
- [REQ-AG-103] When all the AgentTasks of a deliverable are in the DONE state, the system shall execute the [pull-reqest] prompt with a minimum complextity of 4
- [REQ-AG-104] When a deliverable has multiple AgentTasks and any one of them is in the [Failed], [Rejected] or [NeedsReview] state, change the Deliverable State to [Failed]
- [REQ-AG-105] When the system invokes OpenCode, it shall use the least cost model that has a complexity value equal or higher of what is required

## State-Driven Requirements
- [REQ-AG-299] While the system finds a Deliverable for the current project in one of the states recognized by the [Deliverable state transitions] table, it shall execute the appropriate prompt from the [Deliverable state transitions]
- [REQ-AG-291] While the system finds an AgentTask for the current project in one of the states recognized by the [AgentTask state transitions] table, it shall execute the appropriate prompt from the [AgentTask state transitions]

## Unwanted Behavior Requirements
- [REQ-AG-300] When an ```opencode.json``` file is present, the system shall not delete or overwrite the file or delete existing content from it

# Opencode

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

## Opencode providers

```
    "provider": {
        "OpenRouter": {
            "name": "OpenRouter",
            "npm": "@ai-sdk/openai-compatible",
            "models": {
                "minimax/minimax-m2.7": {
                    "name": "minimax/minimax-m2.7"
                },
                "openai/gpt-5.4": {
                    "name": "openai/gpt-5.4"
                }
            },
            "options": {
                "baseURL": "https://openrouter.ai/api/v1",
                "apiKey": "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"
            }
        }
    }
```

## Deliverable state transitions

| Current state | Deliverable Type | Prompt     | Min complexity | Future state |
| ------------- | ---------------- | ---------- | -------------- | ------------ |
| Design        | Spike            | research   | 10             | Done         |
| Design        | Feature          | design     | 10             | Plan         |
| Plan          | Defect           | root-cause | 8              | Implement    |
| Plan          | Feature, Maintenance | plan   | 8              | Implement    |
| Merge         | (all)            | merge      | 8              | Test         |

## AgentTask state transitions

| Current state | Prompt      | Min complexity | Future state |
| ------------- |  ---------- | -------------- | ------------ |
| Ready         | implement   | 4              | Done         |

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
| Syntax      | Description |
| ----------- | ----------- |
| Header      | Title       |
| Paragraph   | Text        |
# Technical specification

- [OpenCode permissions](https://opencode.ai/docs/permissions)
- [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/lang-spec/chapter-15?view=powershell-7.6)
- [Powershell guidelines](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines?view=powershell-7.6)

# Quality gates

```Get-Command -syntax '.\scripts\devstack.ps1'```

Skip any other tests as this is a script

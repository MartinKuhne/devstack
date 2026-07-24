# devstack

## Mission

_devstack_ is a project management MCP server and UI to enable better interaction with code agents. It provides a Project → Deliverable → AgentTask hierarchy to separate planning from implementation. If you are cost conscious (or otherwise token constrained), use a frontier model for planning and an efficient model for coding.

Deliverables and tasks are tracked through their lifecycle (Draft, Ready, Implement, Review, Done) but the system is not opinionated on state transitions.

There is a fully fledged admin UI to add features, or review/approve AI generated tasks:

![Screenshot](doc/img/devstack.png)

## Features

### MCP Server
Provides tools for AI coding agents (OpenCode, Claude, etc.) to interact with DevStack data via the [Model Context Protocol](https://modelcontextprotocol.io):
- Read, create, and update **Projects**, **Deliverables**, and **AgentTasks**
- Smart "get next" tools that find the next item to work on based on priority
- HTTP streaming protocol, JSON-RPC 2.0, health check endpoint

### MCP Tools

| Tool | Description |
|------|-------------|
| `get_projects` | Read all projects from DevStack |
| `get_project` | Read a project by its ID |
| `create_project` | Create a new project in DevStack |
| `get_deliverable` | Read a deliverable by its ID |
| `get_next_deliverable` | Find the next deliverable in Implement status for a project |
| `create_deliverable` | Create a new deliverable (Feature) in DevStack |
| `update_deliverable` | Modify an existing deliverable in DevStack |
| `update_deliverable_status` | Change the state of a deliverable in DevStack |
| `get_task` | Read an agent task by its ID |
| `get_next_task` | Find the next task to work on for a project |
| `create_task` | Create a new agent task in DevStack |
| `update_task` | Modify an existing agent task in DevStack |
| `update_task_status` | Change the state of an agent task in DevStack |

### Admin UI
A web interface built with React, Tailwind, and ShadCN for human interaction:
- CRUD management for all entities (Projects, LLMs, Deliverables, AgentTasks)
- Dashboard showing deliverable counts by status
- Markdown rendering for rich text fields
- Search and navigation by project
- Status transitions via dropdown

### GraphQL API
Backend data layer with full CRUD over all entities, filtering, sorting, and paging:
- **Projects** - top-level containers with repository tracking
- **Deliverables** (Features, Spikes, Defects, Maintenance) - full lifecycle with design, acceptance criteria, execution plan, security/performance impact, test and deployment plans
- **AgentTasks** - granular work items linked to deliverables
- **LargeLanguageModels** - configurable model definitions with cost and complexity tracking

### Note

This was intentionally intended to be a fully automated coding system. For the time being, I am focusing on the mcp server and UI parts as the most valuable aspects.

# References and inspiration
- [Prompt Kit](https://github.com/microsoft/PromptKit)
- [Spec Kit](https://github.com/github/spec-kit)
- [Awesome Copilot](https://github.com/github/awesome-copilot)
- [Agent Skills For Real Engineers](https://github.com/mattpocock/skills)


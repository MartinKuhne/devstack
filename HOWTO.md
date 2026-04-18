In it's current state, the easiest way to try this out is to have it modify itself.

# Warning

There is no authentication in this iteration. If someone creates an EPIC to sell all your possessions, it probably will.

# Prerequisites

- This was built and tested on Windows. Most if not all should work on Linux, but you'll need Powershell.
- Git
- Clone the repository
- Docker (or equivalent)
- Opencode
  - Opencode needs to be configured with a model. You can use one of their free models, or plug in a local LLM.
  - The project intends to manage agents and models, but that's not in yet.

# Installation

Run

```
docker compose up --build -d
scripts/DevStack.ps1 init
```

This will create a project, and create a local ```opencode.json``` to configure the MCP server

Then, head to http://localhost:8087 for the admin UI

# Operation

Let's first look at how this is set up. The key idea is to have a system to house a project/epic/feature/task structure that's accessible to both an AI coding agent and a human. The human will use the admin UI and the coding agent is provided an MCP server.

Go ahead an enter a feature, and then run

```
scripts/DevStack.ps1 run
```

It will loop over the planning tasks, then execute tasks.

# Restrictions

There is no management of repos or pull requests, it will make changes to whatever is in your current directory and commit changes when they are done. There are no limits and no cost management, it can and will run day it night until all the tasks are complete. (That was kind of the point.)
In it's current state, the easiest way to try this out is to have it modify itself.

# Warning

- This is primarily a research project. YMMV.
- There is no authentication in this iteration. If someone creates an EPIC to sell all your possessions, it probably will.
- It WILL churn through tokens in a forever loop. You want it to code all night, it will code all night.

# Prerequisites

- This was built and tested on Windows. Most if not all should work on Linux, but you'll need Powershell.
- Git
- Docker (or equivalent)
- Opencode
  - Opencode needs to be configured with a model. You can use one of their free models, or plug in a local LLM.
  - The project intends to manage agents and models, but that's not in yet.
- Github
- Clone this repository

# Installation

Create an .env file. Using secrets 'abc' and 'def' in this example

```
DEVSTACK_SECRET_KEY=abc
ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=devstack;Username=devstack;Password=def"
POSTGRES_PASSWORD=def
```

Run

```
docker compose up --build -d
scripts/DevStack.ps1 init
scripts/DevStack.ps1 run
```

This will create a project, and create a local ```opencode.json``` to configure the devstack MCP server

Then, head to http://localhost:8087 for the admin UI. Create a Defect or Feature. The agent will not touch anyting that's in DRAFT status. Change the status to DESIGN, or PLAN (or IMPLEMENT if you want to skip the earlier stages) for the agent to work on the code.

# Use with projects (not tested)

Copy the contents of the scripts directory into your project. 

```
scripts/DevStack.ps1 init
scripts/DevStack.ps1 run
```

# Operation

Let's first look at how this is set up. The key idea is to have a system to house a project/epic/feature/task structure that's accessible to both an AI coding agent and a human. The human will use the admin UI and the coding agent is provided an MCP server.

## Requirements in the admin UI

Visit the admin UI and enter a feature or defect. 

## Executing the changes

(if not already running)

```
scripts/DevStack.ps1 run
```

It will loop over the planning tasks, then execute tasks.

# Restrictions

There is no management of repos or pull requests, it will make changes to whatever is in your current directory and commit changes when they are done. There are no limits and no cost management, it can and will run day it night until all the tasks are complete. (That was kind of the point.)
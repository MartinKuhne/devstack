# devstack

When I started experimenting with local LLMs for coding, I quickly discovered that they are very slow andnot very capable. You may have a free resource but actually using that resource requires different patterns. Having gone thought a few coding projects,
I realized I needed a framework to analyze requirements, break them down into scoped work items that can be performed by a local LLM, and to run this in a loop until all the work is done. It also needs to have a library of prompts on hand to perform a variety of tasks.

And, to be fair, a vibe coding project to do more vibe coding? What could go wrong?

# Dream

The dream is to have a visual tool where the user enters their requirements, and a set of agents divide up the work then perform the work without a lot of intervention. Tasks can be held in planning if there are open questions.

# Stack

The project can't build itself unfortunately (maybe it will be able to improve itself). My coding agent is opencode and I have the following tools installed to help with the coding
- saga: The closest I could find to meet my requirements. Models epics, stories and tasks internally.  On the initially planning pass, gpt-5.4 generated more than a hundred tasks to execute. (saga-mcp)
- codebase-memory: Indexes the codebase and also has an architecture tool call
- context7: Code samples for many use cases
- dotnet: Dotnet sdk for solutions, projects, test runs and nuget (Community.Mcp.DotNet)
- fetch: web fetch (mcp-server-fetch)
- filesystem: better access to local files (@modelcontextprotocol/server-filesystem)
- git: naturally (mcp-server-git)
- refactor: bulk edits, not sure how to get opencode to use this more (@myuon/refactor-mcp@latest)

The ```build.ps1``` script runs opencode in a loop with a prompt to work on the next task

# Models

I think there are a couple of useful tiers to be considered

- A top of the line model (I like GPT 5.4) to do the planning
- A midrange model to do higher complexity work, hopefully at 10% of the cost (I like MiniMax 2.7)
- A larger local LLM. Quen3 coder next seems to be a reasonable compromise between speed, capability and stability. I wanted to try gemma 4 but it crashes every 5 minutes under llama.cpp.
- I am not sure if there is an edge model that can run coding tasks and tool calls. So far it has been discouraging. Hopefully eventually a model can be found to run on 8 or 16gb VRAM.

The core planning prompt is to split the work into tasks that can be performed by an AI coding agent in under 20 minutes, to be specific about what is to be accomplished, and to rate the complexity.

# Will it work?

Maybe, maybe not. I think there is a lot of valuable experience to be had to move on from chat prompts, and to put some thoughts into really describing all the detail on how the work is to be done. 

# V1

Qwen3.5-122B-A10B declared it production ready after completing the initial 100 tasks that came out of ```SPEC.md```. There were a couple of initial isses
- Agent code had a few typescript issues. That was avoidable
- There was a runtime error with BullMQ related to the redis configuration. That's pretty understandable, only discoverable at runtime
- CORS needed to be configured for graphql to be accessible from a browser
- An environment variable was set to point the admin UI to the graphql endpoint, but browser code can't use environment variables. Fair.
- There is another issue with BullMQ that I need to look into
- There is an issue with GraphQL "Unable to infer or resolve the type of field Feature.validStatusTransitions.". Definitely should have asked for live integration tests.

The implementation comes with a fair bit of complexity. There is a full work queue system for the coding agent, react lazy loading, a dead letter queue ... 

A variety of "open questions" documents were created, as well as a top level package.json (?)

# Pivot

As it turns out, it wasn't really working. The Admin UI was built on a ```schema.graphql``` that it hand created, instead of the actual schema it should have retrieved from the server. React/Apollo has really tight coupling between the generated classes and the code, so most of it was just wrong.

I also realized there was no need to invent another coding agent. That is not a unique value add. I experimented a little more with how I run opencode, and I will likely discontinue the coding agent. The design was pivoted for the graphql server to also be an _MCP Server_ to facilitate interaction with a coding agent.

The saga MCP server continues to be very valuable. It does not have an UI or a feedback loop to restart tasks that have open questions.

# devstack progression

# Iteration 4

The project has some structural deficiencies related to using coding agents
- The Hot Chocolate (graphQL) library is implemented as a code-first approach. That's great for getting results quickly, however there is no schema.graphql for clients to consume directly
- The Hot Chocolate online documentation mixes schema-first and code first and isn't really machine readable
- I failed to convince AI to deal with Strawberry Shake (a C# code generator) because it can't find the generated code (it's generated during the build process behind the scenes)
- AI could not deal with tests executing against a locally deployed instance and it could not figure out testcontainers without some serious intervention
- There is no GraphQL standard for filtering, sorting or paging.
- The Hot Chocolate [UseFiltering] and [UsePaging] attributes are terribly convenient for developers, no meat on the bones for AI
- The models I have at my disposal aren't particuarly adept at writing Powershell, and I haven't gotten over my dislike for Python.
- I made a lot of changes to the schema over time

These factors combined have made it really time-consuming and error prone to keep integration tests, the MCP server, the runner, the admin UI and the admin UI tests up to date. I tried to produce a schema.graphql derived from the Hot Chocolate implementation but then that was not fully correct and any errors propagate through the system quickly. I also procured an alternative implementation on node.js/apollo which I thought would be a better fit for a schema-first approach, however I haven't had a chance to test that. I haven't decided on the best path forward for the API surface.

I have the MCP server use the DbContext for writes and the commands for writes.

# Iteration 3

When I started this out, I might have assumed we are writing code for human consumption and modification, and I modeled a human-centric, incremental workflow. That's not wrong, but also not entirely right. In a greenfield project, the specification is the the source of truth, the code is disposable, and if the code is not correct, we need to revise the specification until we get the intended results.

I ran such prompts for a night, and it has seen some success. The agent made incremental changes that closed gaps between the specification and the implementation. I had substantially revised and simplified the data model and made the user interface specification more clear, so that was a good test. However, the code agent considers the entire source code as the truth, and it's been difficult to get it to just implement the specification. At some point it found a stale schema.graphql with the tests and proceeded to change the server to match that schema (despite the instructions to consider the spec the source of truth). Furthermore, no prompt has been successful to get it to write verifiably correct, tested code.

Qwen/Qwen3.6-35B-A3B has been a real delight. It runs at twice the speed of Qwen/Qwen3.5-122B-A10B with comparable results.

The "Please make the code so it matches the spec" approach is very token intensive. I go through 250m prompt and 1m generated tokens a day. For now I brought the codebase memory mcp back, maybe that will speed up the analysis stage. My head is full of ideas for a better workflow but I also want to reach a stage where the project is fully functional. 

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

"4. Removed broken test project - The DevStack.Tests.Integration.GraphQL.Client project had fundamental design issues: it mixed domain types (DevStack.Domain.Enums) with StrawberryShake-generated types. This caused 25 compilation errors that couldn't be easily fixed without rewriting all the tests. Removed the entire project rather than try to fix it."

Right - can we build some accountability into these models :)


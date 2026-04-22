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

# MCP Specification

# Goals

Enable an AI coding agent and coding tool to access the devstack data

# Components

- MCP Server
- Unit tests
- Integration tests, using JSON RPC wire protocol with no knowledge of the MCP server process, and using Testcontainers for .NET
  - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures
  - (run with ```dotnet test src/server/```)

# EARS (Easy Approach to Requirements Syntax) formatted functional requirements

- [MCP-001] The MCP server shall support the tool calls as per the Tool call section below
- [MCP-002] The MCP server shall be deployable as a container
- [MCP-003] The MCP server shall use the http streamable protocol
- [MCP-004] The MCP server shall access and store data using the shared DbContext
- [MCP-005] The MCP server shall create AgentTasks in the READY state
- [MCP-006] The system shall accept MCP requests at the /mcp endpoint

- [MCP-050] The system shall apply [McpServerToolType] on classes containing related tools.
- [MCP-051] The system shall apply [McpServerTool(Name = "tool_name")] with snake_case naming convention.
- [MCP-052] The system shall add [Description] attributes to all tools and their parameters.
- [MCP-053] The system shall organize related tools into separate classes.
- [MCP-054] Tools shall return string or JSON-serializable objects.
- [MCP-055] Tools shall format output as Markdown for readability by LLMs.
- [MCP-056] The system shall include usage hints in tool output where applicable.
- [MCP-057] If a tool needs to interact with the client's LLM, it shall use McpServer.AsSamplingChatClient().
- [MCP-058] Tools shall support async operations with proper CancellationToken usage.

- [MCP-070] The agent shall apply [McpServerPromptType] on classes containing related prompts.
- [MCP-071] 5.2	The agent shall apply [McpServerPrompt(Name = "prompt_name")] with snake_case naming convention.
- [MCP-072] 5.3	One prompt class shall contain only one prompt.
- [MCP-073] 5.4	Prompt methods shall return ChatMessage (not string).
- [MCP-074] 5.5	Prompts shall use ChatRole.User to represent user instructions.
- [MCP-075] 5.6	The agent shall add [Description] attributes to all prompts and their parameters.
- [MCP-076] 5.7	Prompts shall accept optional parameters with default values for flexible customization.

- [MCP-100] The MCP Server shall communicate using the JSON-RPC 2.0 protocol format.
- [MCP-101] The MCP Server shall strictly adhere to the official Model Context Protocol specification for message structure.
- [MCP-102] When the client sends an initialize request, the server shall respond with a ServerCapabilities object defining its supported features (e.g., resources, tools, prompts).
- [MCP-103] When the client sends an initialized notification, the server shall begin accepting operational requests.
- [MCP-104] When the client requests the list of available tools, the server shall return an array containing the name, description, and JSON Schema for input validation of each tool.
- [MCP-105] When the client calls a specific tool by name, the server shall execute the corresponding logic with the provided arguments.
- [MCP-106] If the input arguments provided for a tool do not match the declared JSON Schema, the server shall return an InvalidParams error.
- [MCP-107] If the client attempts to call a tool that does not exist, the server shall return a MethodNotFound error.
- [MCP-108] If an internal server error occurs during request processing, the server shall return a JSON-RPC error object with a code of -32603.

- [MCP-200] The system shall expose an HTTP GET endpoint at the path /health as a health check
- [MCP-201] The system shall return all health check responses in JSON format.
- [MCP-202] If the request targets the /health endpoint, the system shall not require authentication or authorization headers.
- [MCP-203] While the system and all its critical dependencies are operational, the system shall respond with the HTTP status code 200 OK.
- [MCP-204] When the system detects a critical failure (e.g., database connection loss), it shall respond with the HTTP status code 503 Service Unavailable.
- [MCP-205] When the health check is executed, the system shall attempt to open a connection to the primary database.

- [MCP-300] The system shall be tested by a complete set of unit tests
- [MCP-301] The system shall be tested by a complete set of integration tests
- [MCP-302] When the system creates test data, it shall mark it as such by using the "[DeleteAfterTest]" text in the Title or Name of the object created

- [MCP-400] When the create_deliverable tool is called with a missing, empty, invalid, or not found ProjectId, the system shall fail the tool call
- [MCP-401] When the create_task tool is called with a missing, empty, invalid, or not found ProjectId, the system shall fail the tool call
- [MCP-403] When the create_task tool is called with a missing, empty, invalid, or not found DeliverableId, the system shall fail the tool call

# Tool calls

| Name          | Operation                                                 |
|---------------|-----------------------------------------------------------|
| get_projects  | Read all Projects (Fields: Name, Id, Repository)  |
| get_project   | Read project by ID (Fields: Name, Repository) |
| create_deliverable | Create Deliverable (Fields: ProjectId (required), Status, Title (required), Description (required), AcceptanceCriteria, ExecutionPlan, SecurityImpact PerformanceImpact, TestPlan, DeploymentPlan) |
| get_deliverable | Read Deliverable |
| update_deliverable | Update Deliverable (Fields: Description, AcceptanceCriteria, ExecutionPlan, SecurityImpact, PerformanceImpact, TestPlan, DeploymentPlan, AgentFeedback, Blocking) |
| update_deliverable_state | Change Deliverable state |
| create_task | Create AgentTask (Fields: ProjectId (required), DeliverableId (required), Title (required), Status, Description (required)) |
| get_task | Read AgentTask |
| update_task | Update AgentTask (Fields: Status, Result, Errors, CommitHash, Agent) |
| update_task_state: Change AgentTask state |

Include [Global non-functional requirements](../NON-FUNCTIONAL.md)

# Technical specification

- [MCP Specification](https://modelcontextprotocol.io/specification/2025-11-25)
- [MCP Protocol](https://modelcontextprotocol.io/specification/2025-11-25/basic/transports)
- [MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [TestContainers](https://dotnet.testcontainers.org/)

# Quality gates

```dotnet build ./src/Server/DevStack.Mcp```
```dotnet test ./src/Server/DevStack.Tests.Integration.MCP```
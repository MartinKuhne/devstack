# MCP Specification

# Goals

Enable an AI coding agent and coding tool to access the devstack data

# Components

- Model Context Protocol Server, Containerized, .net with Postgress access though shared DbContext with GraphQL
- Integration tests, using JSON RPC wire protocol with no knowledge of the MCP server process, and using Testcontainers for .NET
  - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures
  - (run with ```dotnet test src/server/```)

# Capabilities

- Read all Projects (Fields: Name, Id, Repository)
- Read project by ID (Fields: Name, Repository)
- Create Deliverable
- Modify Deliverable
- Change Deliverable state
- Create AgentTask
- Modify AgentTask
- Change AgentTask state

Newly created Deliverables and Agent Tasks are created in the READY state

# Technical specification

- [MCP Specification](https://modelcontextprotocol.io/specification/2025-11-25)
- [MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- Log operations and errors to the console


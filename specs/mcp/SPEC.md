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

- Model Context Protocol Server, Containerized, .net with Postgress access though shared DbContext with GraphQL
- Use the http streamable protocol
- Integration tests, using JSON RPC wire protocol with no knowledge of the MCP server process, and using Testcontainers for .NET
  - The integration tests MUST run and MUST PASS do not make excuses about pre existing failures
  - (run with ```dotnet test src/server/```)

# Capabilities

- get_projects: Read all Projects (Fields: Name, Id, Repository)
- get_project: Read project by ID (Fields: Name, Repository)
- create_deliverable: Create Deliverable
- get_deliverable: Read Deliverable
- update_deliverable: Update Deliverable
- update_deliverable_state: Change Deliverable state
- create_task: Create AgentTask
- get_task: Create AgentTask
- update_task: Update AgentTask
- update_task_state: Change AgentTask state

Newly created Deliverables and Agent Tasks are created in the READY state as they should be ready for execution when they are created by a code or planning agent

# Technical specification

- [MCP Specification](https://modelcontextprotocol.io/specification/2025-11-25)
- [MCP Protocol](https://modelcontextprotocol.io/specification/2025-11-25/basic/transports)
- [MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [TestContainers](https://dotnet.testcontainers.org/)

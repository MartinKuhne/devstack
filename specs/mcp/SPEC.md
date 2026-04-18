# MCP Specification

# Goals

Enable an AI coding agent and coding tool to access the devstack data

# Components

- Model Context Protocol Server, Containerized, .net with Postgress access though shared DbContext with GraphQL
- Integration tests, using JSON RPC wire protocol with no knowledge of the MCP server process

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

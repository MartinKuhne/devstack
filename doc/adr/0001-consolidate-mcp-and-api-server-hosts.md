# 1. Consolidate MCP Server and GraphQL API Server Hosts

* **Status**: Accepted
* **Date**: 2026-07-25
* **Authors**: DevStack Architecture Team

## Context

DevStack previously operated two separate ASP.NET Core host services:
1. `DevStack.Api`: Serving GraphQL operations at `/graphql` and REST health endpoints.
2. `DevStack.Mcp`: Serving Model Context Protocol tool and resource handlers for AI agents at `/mcp`.

Both services are written in .NET 10, share identical underlying domain logic (`DevStack.Application`, `DevStack.Domain`, `DevStack.Infrastructure`, `DevStack.Persistence`), and connect to the same PostgreSQL database (`DevStackDbContext`).

Operating these services as distinct container microservices incurred significant cloud hosting overhead:
* Running two separate container instances 24/7 across Test (Staging) and Production environments doubled baseline compute and memory costs.
* Running duplicate .NET runtime processes incurred double JIT compilation and memory baseline overhead (~150MB–200MB RAM per host instance).
* Dual host containers created two separate Npgsql database connection pools competing for PostgreSQL connection limits.
* Deployment pipelines required building, tagging, scanning, and deploying two distinct container images (`devstack-api` and `devstack-mcp`).

## Decision

We consolidate the MCP server endpoints (`/mcp`) directly into the `DevStack.Api` unified host application.

Specifically:
1. `DevStack.Api` imports `ModelContextProtocol.AspNetCore` and references `DevStack.Mcp`.
2. `DevStack.Api` registers MCP server services (`AddMcpServer()`, tools, prompts, resources, filters, and exception handling middleware).
3. `DevStack.Api` maps the `/mcp` route alongside `/graphql` within the same ASP.NET Core web host process.

`DevStack.Mcp` remains a clean module containing MCP tool definitions, prompts, resources, and DTOs.

## Consequences

### Positive
* **Financial Savings**: Eliminates 50% of container compute overhead across environments, saving **~$20 – $35 / month** (~30% – 45% reduction in total cloud hosting costs).
* **Single Database Connection Pool**: Shared EF Core `Npgsql` connection pool across both GraphQL and MCP operations, optimizing PostgreSQL resource consumption.
* **Simplified CI/CD Pipeline**: Reduces build and deployment artifacts to a single unified Docker container image (`devstack-api` / `devstack-server`).
* **Lower Baseline Memory**: Saves ~200MB RAM per environment by avoiding duplicate .NET 10 runtime instances.

### Negative / Risks
* **Coupled Failure Domain**: An unhandled process crash in MCP tool execution will also affect GraphQL endpoints (mitigated by robust exception handling middleware and stateless request handling).
* **Shared Resource Allocation**: CPU and memory are shared between AI agent tool invocations and GraphQL requests within the same container instance.

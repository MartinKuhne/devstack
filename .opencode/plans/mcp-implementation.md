# MCP Server Implementation Plan

## Gap Analysis: SPEC.md vs. Current Implementation

### What SPEC.md Requires

**Capabilities:**
1. Read all Projects (Fields: Name, Id, Repository)
2. Read project by ID (Fields: Name, Repository)
3. Create Deliverable
4. Modify Deliverable
5. Change Deliverable state
6. Create AgentTask
7. Modify AgentTask
8. Change AgentTask state

**New items created in READY state.**

**Technical:**
- Containerized .NET MCP server
- Shared DbContext with GraphQL (PostgreSQL)
- JSON-RPC wire protocol
- Log operations and errors to console

### What Currently Exists

| Area | Status |
|------|--------|
| `DevStack.Mcp/` | Empty shell (only `obj/`, no `.csproj`, no source files, not in solution) |
| GraphQL layer | Full CRUD + transitions for Projects, Features/Items (Deliverables), Defects, Tasks |
| CQRS handlers | All command handlers in `DevStack.Infrastructure` |
| Domain services | `ItemStatusTransitionService`, `DeliverableStatusTransitionService`, `TaskStatusTransitionService` |
| Test infrastructure | 18 SpecFlow feature files + test client + Testcontainers setup (NOT in solution) |

### What is Missing

1. **`DevStack.Mcp` project** — No `.csproj`, no source code, not referenced in solution
2. **MCP NuGet packages** — `ModelContextProtocol`, `ModelContextProtocol.AspNetCore`
3. **MCP tool definitions** — All 8 tools from SPEC.md
4. **HTTP transport endpoint** — `/mcp` endpoint for JSON-RPC
5. **Docker containerization** — Dockerfile + docker-compose for MCP server
6. **Test feature alignment** — Current test features expect `devstack_createFeature`/`devstack_deleteFeature` etc. — must be updated to match SPEC.md tool names

---

## Implementation Steps

### Step 1: Create `DevStack.Mcp` Project (Complexity: 3)

Create a new .NET 10.0 class library project:
- `src/Server/DevStack.Mcp/DevStack.Mcp.csproj`
- Add NuGet packages: `ModelContextProtocol` v1.2.0, `ModelContextProtocol.AspNetCore` v1.2.0
- Add project references to: `DevStack.Application`, `DevStack.Infrastructure`, `DevStack.Domain`
- Add to solution via `dotnet-solution Add`
- Add `McpServer` entry point with ASP.NET Core hosting

### Step 2: Implement MCP Tools (Complexity: 8)

Create tool classes using `[McpServerToolType]` / `[McpServerTool]` attributes.

**Tools to implement (matching SPEC.md exactly):**

| Tool Name | Purpose | Parameters | Returns |
|-----------|---------|------------|---------|
| `devstack_readProjects` | List all projects | `first`?, `skip`? | `IEnumerable<ProjectSummary>` (Name, Id, Repository) |
| `devstack_getProjectById` | Get single project | `id` (Guid) | `ProjectDetail` (Name, Repository) |
| `devstack_createDeliverable` | Create a deliverable | `projectId`, `title`, `description`? | `DeliverableSummary` (with Id) |
| `devstack_updateDeliverable` | Modify deliverable | `id`, `title`?, `description`? | `DeliverableSummary` |
| `devstack_transitionDeliverableStatus` | Change deliverable state | `id`, `targetStatus`, `actor` | `DeliverableSummary` |
| `devstack_createAgentTask` | Create an agent task | `featureId`?, `title`, `deliverable`? | `AgentTaskSummary` (with Id) |
| `devstack_updateAgentTask` | Modify agent task | `id`, `title`?, `deliverable`? | `AgentTaskSummary` |
| `devstack_transitionAgentTaskStatus` | Change agent task state | `id`, `targetStatus`, `actor` | `AgentTaskSummary` |

**Implementation approach:**
- Use dependency injection to inject `DevStackDbContext` or CQRS handlers
- Return stringified JSON for structured results (MCP tool return convention)
- New deliverables/tasks created in READY state
- Use `DevStack.Infrastructure` CQRS handlers (reuse existing `CreateFeatureCommand`, `CreateTaskCommand`, etc.)
- Map: Deliverable = Item subtype Feature, AgentTask = Item subtype Task

### Step 3: Register Tools & Configure Transport (Complexity: 3)

In `Program.cs` or server configuration:
```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithToolsFromAssembly();
app.MapMcp();
```
- Register Serilog console logging for operations and errors
- Server name: "DevStack MCP Server"
- Protocol version: "2024-11-05"

### Step 4: Add Dockerfile & Docker Compose (Complexity: 4)

- Create `src/Server/DevStack.Mcp/Dockerfile` (multi-stage build for .NET)
- Update `docker-compose.yml` to include MCP server service
- MCP server connects to same PostgreSQL as GraphQL server

### Step 5: Update Test Features (Complexity: 4)

Update SpecFlow feature files to match SPEC.md tool names:
- Rename `devstack_createFeature` → `devstack_createDeliverable`
- Rename `devstack_updateFeature` → `devstack_updateDeliverable`
- Rename `devstack_transitionFeatureStatus` → `devstack_transitionDeliverableStatus`
- Rename `devstack_getFeatureById` → `devstack_getDeliverableById`
- Rename `devstack_getFeatures` → `devstack_getDeliverables`
- Rename `devstack_deleteFeature` → `devstack_deleteDeliverable` (if needed)
- Rename `devstack_getValidStatusTransitions` → `devstack_getValidDeliverableStatusTransitions`
- Rename `devstack_createTask` → `devstack_createAgentTask`
- Rename `devstack_updateTask` → `devstack_updateAgentTask`
- Rename `devstack_transitionTaskStatus` → `devstack_transitionAgentTaskStatus`
- Rename `devstack_getTaskById` → `devstack_getAgentTaskById`
- Rename `devstack_getTasks` → `devstack_getAgentTasks`
- Rename `devstack_deleteTask` → `devstack_deleteAgentTask`
- Rename `devstack_getProjectById` → `devstack_getProjectById` (same)
- Rename `devstack_getProjects` → `devstack_readProjects`
- Rename `devstack_updateProject` → `devstack_updateProject` (same)
- Rename `devstack_deleteProject` → `devstack_deleteProject` (same)

Update step definitions (`DevStackToolsSteps.cs`) to use new tool names.

### Step 6: Add MCP Test Project to Solution (Complexity: 1)

Add `DevStack.Tests.Integration.MCP` to `DevStack.slnx` so it builds and runs.

### Step 7: Build & Test (Complexity: 2)

- `dotnet build src/server` — no warnings/errors
- `dotnet test src/server` — all tests pass
- `docker compose build` — MCP image builds successfully

---

## Dependencies

- **External:** `ModelContextProtocol` v1.2.0, `ModelContextProtocol.AspNetCore` v1.2.0
- **Internal:** Reuses existing CQRS handlers in `DevStack.Infrastructure`, `DevStackDbContext` from `DevStack.Infrastructure.Persistence`
- **Test:** Existing SpecFlow test infrastructure, Testcontainers for PostgreSQL

## Risk Assessment

| Risk | Mitigation |
|------|-----------|
| ModelContextProtocol SDK API changes | Pin to specific version v1.2.0 |
| DbContext sharing between GraphQL and MCP | Both connect to same PostgreSQL via connection string |
| Test feature renames break existing tests | Update all step definitions in parallel |
| Container networking | Use docker-compose service names for DB connection |

## Complexity: 7/10

Moderate complexity. The main effort is implementing 8 MCP tools that wrap existing CQRS handlers, updating test features to match SPEC.md naming, and containerization.

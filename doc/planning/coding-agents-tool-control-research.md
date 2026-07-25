# Market Research: Fine-Grained Tool Control & Workspace Sandboxing in AI Coding Agents

## Executive Summary

This research note evaluates current market solutions (frameworks, CLI tools, harness platforms, and SDKs) for building an **automated coding agent** that requires strict access control. 

### Core Requirement
The target agent must be **strictly confined to source code directory operations** (reading, writing, searching, listing, and diffing files within a designated workspace directory) and **completely denied access to the operating system**, shell environments (Bash, PowerShell, zsh), subprocess invocation, or network/system administration utilities.

### Key Finding
Coding agents fall into two major architectural paradigms regarding tool permissions:

1. **Interactive Developer CLI Harnesses** (e.g., *Claude Code*, *Aider*): Designed around developer-in-the-loop workflows where shell execution is a fundamental primitive. Tool controls in these tools rely primarily on user confirmation popups, command pattern allowlists, or system prompts. Disabling shell access entirely is either unsupported or significantly degrades the agent's core capabilities.
2. **Programmatic Agent Frameworks & Mode-Based Harnesses** (e.g., *OpenHands SDK*, *LangGraph*, *OpenCode*, *Pi*, *AGY (Antigravity SDK)*, *Roo Code / Cline Custom Modes*, *Goose*, *AutoGen*, *CrewAI*, *Hermes*): Expose hard structural or configuration-level mechanisms to whitelist specific file-system tools and **Model Context Protocol (MCP)** tools while **programmatically excluding or explicitly denying shell/terminal execution tools**.

### Strategic Recommendation for DevStack
To achieve a production-grade, prompt-injection-proof automated coding agent limited strictly to source files, DevStack should adopt one of the following top-tier options:
- **Primary Option (SDK / Ecosystem Native)**: A custom agent using **OpenHands SDK**, **LangGraph**, or **AGY Python SDK (`google-antigravity`)** configured with a restricted tool capabilities set (`CapabilitiesConfig` / `FileManagementToolkit`) scoped strictly to `./src`, coupled with **DevStack MCP Server** tools and zero terminal/shell tools registered.
- **Secondary Option (Configured Harness)**: **OpenCode** or **Roo Code / Cline CLI** running in headless mode with `"permission": { "bash": "deny", "mcp:devstack/*": "allow" }` in `opencode.json` or a custom `.roomodes` file that exposes `mcp`, `read`, and `edit` tool groups while stripping the `command` (`execute_command`) tool group.

---

## Detailed Market Survey of Coding Agents & Frameworks

### 1. Open-Source Agent Frameworks & SDKs (Programmatic Control)

#### 1.1 OpenHands SDK (formerly OpenDevin)
* **Overview**: Modular agent framework and evaluation platform designed for autonomous software engineering.
* **Tool Control Mechanism**:
  - OpenHands provides explicit tool injection when instantiating agents via its Python SDK (`openhands.sdk.Agent`).
  - Instead of passing default tool suites, developers pass an explicit whitelist of tools:
    ```python
    from openhands.sdk import Agent, LLM, Tool
    from openhands.tools.file_editor import FileEditorTool

    # TerminalTool / CmdRunAction is explicitly omitted
    agent = Agent(
        llm=llm,
        tools=[
            Tool(name=FileEditorTool.name),
            Tool.from_mcp("devstack_mcp", "get_next_task")
        ]
    )
    ```
  - Micro-agents and skills can be globally disabled via `config.toml` (`enable_prompt_extensions = false` or `disabled_microagents = [...]`).
* **OS / Shell Isolation**: Exceptional. When `TerminalTool` / `CmdRunAction` is omitted from `tools`, the LLM function schema contains zero command-execution entries. Prompt injection cannot invoke shell commands because the underlying runtime dispatcher has no shell tool registered.
* **MCP Integration**: Fully supported via SDK MCP client wrappers.
* **Workspace Confinement**: High. Supports volume mounting and root directory scoping.
* **Fit for DevStack**: **10 / 10** (Ideal for building headless, file-only background subagents).

#### 1.2 LangChain / LangGraph
* **Overview**: Industrial-standard graph-based orchestration framework for stateful AI agents.
* **Tool Control Mechanism**:
  - Provides declarative tool binding via `FileManagementToolkit`.
  - Developers explicitly select allowed tools and enforce directory scoping via `root_dir`:
    ```python
    from langchain_community.agent_toolkits import FileManagementToolkit
    from langchain_mcp import MCPToolkit

    file_tools = FileManagementToolkit(
        root_dir="./src",
        selected_tools=["read_file", "write_file", "list_directory", "file_search"]
    ).get_tools()

    mcp_tools = MCPToolkit(server_name="devstack").get_tools()

    # ShellTool / BashProcessTool is completely omitted
    agent_tools = file_tools + mcp_tools
    ```
* **OS / Shell Isolation**: Exceptional. Hard structural boundary. The LLM receives function declarations solely for file I/O within `root_dir` and typed MCP endpoints.
* **MCP Integration**: Native via `langchain-mcp` package. Converts MCP endpoints into standard `StructuredTool` instances.
* **Workspace Confinement**: High. `root_dir` strictly enforces path canonicalization and blocks path traversal (`../`) attempts.
* **Fit for DevStack**: **10 / 10** (Ideal for custom pipeline integration and deterministic graph execution).

#### 1.3 Antigravity CLI & SDK (`agy`)
* **Overview**: Google Antigravity AI-first development CLI (`agy`) and Python SDK (`google-antigravity`).
* **Tool Control Mechanism**:
  - In CLI mode (`agy`), uses structured permission management (`settings.json` and fine-grained `ask_permission` policy matching per tool action and target).
  - In Python SDK mode (`google.antigravity`), tool capabilities are configured programmatically via `CapabilitiesConfig` and `LocalAgentConfig`:
    ```python
    from google.antigravity import Agent, LocalAgentConfig, CapabilitiesConfig

    # Run with restricted capabilities (command execution tool omitted)
    config = LocalAgentConfig(
        system_instructions="Code modification assistant.",
        capabilities=CapabilitiesConfig(
            # Command tools excluded; MCP tools explicitly granted
        )
    )
    async with Agent(config) as agent:
        # Agent execution loop
    ```
* **OS / Shell Isolation**: High. Fine-grained tool action permission enforcement (`read_file`, `write_file`, `command`, `mcp`).
* **MCP Integration**: Native. MCP servers declared under `mcpServers` in `settings.json` or registered programmatically. Fine-grained permission rules (`Action: mcp`, `Target: devstack/*`).
* **Workspace Confinement**: High. Bounded by workspace settings and fine-grained target path permissions.
* **Fit for DevStack**: **9.5 / 10** (Native alignment with AGY ecosystem and Python SDK agent leasing).

#### 1.4 Microsoft AutoGen / AutoGen v0.4 (Magentic-One)
* **Overview**: Multi-agent conversation framework for complex workflows.
* **Tool Control Mechanism**:
  - Execution capability is governed by `code_execution_config` and tool registration.
  - Setting `code_execution_config=False` on `UserProxyAgent` or omitting execution backends (Docker/LocalCommandLine) completely disables code/shell execution.
  - Tools are registered granularly per agent using `@agent.register_for_execution`.
* **OS / Shell Isolation**: Very High. Disabling the execution backend stops all tool call execution requests.
* **MCP Integration**: Supported via custom MCP tool adapters.
* **Workspace Confinement**: Depends on registered custom file functions.
* **Fit for DevStack**: **9 / 10**.

#### 1.5 CrewAI
* **Overview**: High-level multi-agent orchestration framework centered around role-based agents.
* **Tool Control Mechanism**:
  - Declarative tool assignment per agent: `Agent(role="Code Writer", tools=[FileReadTool(), FileWriterTool()])`.
  - Avoids adding `CodeInterpreterTool` or custom shell execution tools.
* **OS / Shell Isolation**: Very High.
* **MCP Integration**: Supported via MCP tool wrappers.
* **Fit for DevStack**: **8.5 / 10**.

---

### 2. Headless CLI Coding Agents & Harnesses (Configuration-Based Control)

#### 2.1 OpenCode
* **Overview**: Open-source CLI agent framework and runtime runner.
* **Tool Control Mechanism**:
  - Configured via `opencode.json` in project root or globally at `~/.config/opencode/opencode.json`, as well as agent frontmatter (`.opencode/agent/<name>.md`).
  - Supports explicit permission rules to block shell access entirely while granting access to DevStack MCP tools:
    ```json
    {
      "mcpServers": {
        "devstack": {
          "command": "dotnet",
          "args": ["run", "--project", "src/Server/DevStack.Mcp/DevStack.Mcp.csproj"]
        }
      },
      "permission": {
        "bash": "deny",
        "mcp:devstack/*": "allow"
      }
    }
    ```
* **OS / Shell Isolation**: High. Setting `"bash": "deny"` in `opencode.json` completely prevents the agent from calling shell execution primitives.
* **MCP Integration**: Native first-class support via `mcpServers`. Fine-grained tool permissions per MCP server/endpoint.
* **Workspace Confinement**: High when restricted via `.opencode/agent` frontmatter and permission settings.
* **Fit for DevStack**: **9.5 / 10** (Strong candidate; `.opencode` configuration directory structure is already established in DevStack).

#### 2.2 Roo Code / Cline (Headless / Custom Modes)
* **Overview**: Popular open-source AI coding assistant supporting VS Code extension and headless CLI execution.
* **Tool Control Mechanism**:
  - Features a **Custom Modes** engine configured via `.roomodes` / `.clinerules` or TOML mode files.
  - Tool access is partitioned into predefined groups: `read`, `edit`, `browser`, `command`, `mcp`.
  - A custom mode can explicitly disable the `command` group (`execute_command`) while enabling `mcp`, `read`, and `edit`:
    ```json
    {
      "customModes": [
        {
          "slug": "devstack-file-coder",
          "name": "DevStack Task Coder",
          "roleDefinition": "You are a code modification agent restricted to file edits and DevStack MCP tools.",
          "groups": ["read", "edit", "mcp"]
        }
      ]
    }
    ```
* **OS / Shell Isolation**: High. Stripping the `command` group removes `execute_command` from the active toolset.
* **MCP Integration**: Native. Configured in `mcpSettings.json` and selectively exposed per custom mode.
* **Workspace Confinement**: Confined to workspace directory opened by the harness.
* **Fit for DevStack**: **9 / 10** (Best option if leveraging an existing IDE/CLI agent harness).

#### 2.3 Pi Agent Harness & Framework
* **Overview**: Lightweight, extensible AI coding harness designed for modular agent workflows.
* **Tool Control Mechanism**:
  - Uses a minimal core architecture, delegating permission control to extension layers (`pi-permission-system`, `pi-permission-layers`, `pi-sandbox`).
  - Disables bash and binds MCP tools in agent definitions:
    ```yaml
    ---
    name: devstack-mcp-agent
    permission:
      tools:
        read: allow
        write: allow
        mcp:devstack/*: allow
        bash: "off"
    ---
    ```
* **OS / Shell Isolation**: High (when `bash: "off"` is declared in frontmatter or `pi-permission-system` extension is enabled).
* **MCP Integration**: Supported via `pi-extension-mcp`.
* **Workspace Confinement**: High when combined with `pi-sandbox`.
* **Fit for DevStack**: **9 / 10**.

#### 2.4 Goose (Block / Square)
* **Overview**: Open-source extensible AI agent CLI built on the Model Context Protocol (MCP).
* **Tool Control Mechanism**:
  - Manages capabilities via extensions defined in `config.yaml` and granular permissions in `permission.yaml`.
  - 100% MCP-native architecture. Every extension in Goose is an MCP server.
  - Users add DevStack MCP server to `config.yaml` and disable the built-in "Developer" extension (which exposes shell execution).
* **OS / Shell Isolation**: High (when developer extension is toggled off).
* **MCP Integration**: Exceptional (Architecture is natively built around MCP).
* **Fit for DevStack**: **8.5 / 10**.

#### 2.5 Hermes (Nous Research)
* **Overview**: Autonomous AI agent framework built by Nous Research.
* **Tool Control Mechanism**:
  - Configuration managed via `~/.hermes/config.yaml` or `hermes config` CLI.
  - Controls tool capabilities at the toolset category level (`hermes tools disable <toolset>`).
  - Supports MCP server integration via custom toolsets.
* **OS / Shell Isolation**: Moderate to High. Shell execution can be disabled by toggling off the terminal toolset or sandboxed inside Docker (`terminal.backend: docker`).
* **MCP Integration**: Supported via MCP toolset adapters.
* **Fit for DevStack**: **7.5 / 10**.

---

## Model Context Protocol (MCP) Server Integration Architecture

### The Strategic Role of MCP in Shell-Less Agents

The Model Context Protocol (MCP) allows AI agents to interact with external services and project management backends via strongly-typed RPC endpoints rather than raw command-line invocations.

In a restricted agent architecture, **MCP serves as the secure alternative to shell execution**:

```
 ┌────────────────────────────────────────────────────────────────────────┐
 │                      RESTRICTED AGENT RUNTIME                          │
 │                                                                        │
 │  Allowed Tool Categories:                                              │
 │    1. Workspace File Tools (read_file, write_file, list_dir, grep)     │
 │    2. DevStack MCP Tools (get_next_task, update_task_status, etc.)     │
 │                                                                        │
 │  EXCLUDED Primitive Categories:                                        │
 │    ❌ OS / Shell Primitives (bash, powershell, exec, cmd, terminal)    │
 └───────────────────┬────────────────────────────────┬───────────────────┘
                     │                                │
                     ▼                                ▼
       ┌───────────────────────────┐    ┌───────────────────────────┐
       │ Workspace Source Directory│    │   DevStack MCP Server     │
       │ (C:\Users\...\src)        │    │  (DevStack.Mcp.csproj)    │
       └───────────────────────────┘    └───────────────────────────┘
```

#### Why MCP + File-Tools is Superior to Shell Execution
1. **No OS Command Execution Risk**: The agent cannot run `rm -rf`, `npm install`, `git push --force`, or arbitrary shell scripts because no shell process launcher is exposed.
2. **Structured Task Workflow**: The agent queries DevStack MCP tools directly (e.g. `get_next_task(projectId: "...")`), reads and updates source files in `./src`, and updates task status (`update_task_status(taskId: "...", status: "Review")`) in a clean, programmatic loop.
3. **Auditability**: Every MCP tool call is logged with explicit JSON-RPC parameters rather than opaque shell strings.

---

## Market Comparison Matrix

| Agent / Framework | Tool Exclusion Granularity | OS/Bash Disabling | MCP Integration Support | MCP Tool Filtering | Path Confinement (`root_dir`) | Prompt Injection Safety | Overall Suitability |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **OpenHands SDK** | Fine (Per Tool) | **Programmatic Exclusion** | Native (`Tool.from_mcp`) | Fine (Explicit Whitelist) | Supported | **Highest** (No Shell Tool) | **10 / 10** |
| **LangGraph / LangChain** | Fine (Per Tool) | **Programmatic Exclusion** | Native (`MCPToolkit`) | Fine (Explicit Whitelist) | Built-in (`root_dir`) | **Highest** (No Shell Tool) | **10 / 10** |
| **OpenCode** | Fine (Pattern / Flag) | **Config (`bash: "deny"`)** | Native (`mcpServers`) | Fine (`mcp:devstack/*`) | Frontmatter / Config | **High** (Config Boundary) | **9.5 / 10** |
| **AGY (`google-antigravity`)** | Fine (Action & Target) | **Programmatic (`CapabilitiesConfig`)**| Native (`mcpServers`) | Fine (`Action: mcp`, Target) | Built-in Policy | **High** (Policy / Schema) | **9.5 / 10** |
| **Roo Code / Cline** | Group & Tool Level | **Config-Based (Remove `command`)** | Native (`mcpSettings`) | Group Level (`mcp` group) | Workspace Bounded | High (Disabled Group) | **9 / 10** |
| **Pi Agent** | Fine (Extension / Frontmatter)| **Frontmatter (`bash: "off"`)** | Extension (`pi-extension-mcp`)| Fine (Frontmatter Rules) | Via `pi-sandbox` | High | **9 / 10** |
| **AutoGen v0.4** | Fine (Per Tool) | **Programmatic (`code_execution=False`)** | Via Adapters | Fine (Function Registration) | Via Custom Handlers | High | **9 / 10** |
| **Goose (Block)** | Extension / MCP Level | **Extension Unloading** | 100% MCP Native Architecture | Extension Level | Custom MCP | High | **8.5 / 10** |
| **CrewAI** | Fine (Per Tool) | **Programmatic Exclusion** | Via Adapters | Fine (Tool List) | Custom Tools | High | **8.5 / 10** |
| **Hermes** | Toolset Category Level | **Toolset Disable / Docker** | Toolset Level | Category Level | Container Scoped | Moderate | **7.5 / 10** |
| **Claude Code** | Permission Pattern | Soft Deny (`deny: ["Bash"]`) | Via Config | Pattern Matching | Project Dir | Moderate (Expects Bash) | **6 / 10** |
| **Aider** | Coarse | Interactive Prompting Only | Limited | N/A | Git Repository | Low (Requires Docker) | **4 / 10** |

---

## Security Architecture: Hard vs. Soft Isolation

When designing an automated coding agent restricted to source files and MCP endpoints, relying on **soft isolation** (system prompts or user confirmation UI) is insufficient.

```
       [ Prompt Injection Attack ]
                   │
                   ▼
  ┌─────────────────────────────────┐
  │         LLM Engine              │
  └────────────────┬────────────────┘
                   │ Attempted Tool Call
                   ▼
 ┌────────────────────────────────────────────────────────┐
 │ SECURITY LAYER COMPARISON                              │
 ├──────────────────────────────────┬─────────────────────┤
 │ SOFT ISOLATION                   │ HARD ISOLATION      │
 │ (System Prompt / Deny Rule)      │ (Programmatic Exclusion)│
 ├──────────────────────────────────┼─────────────────────┤
 │ Shell Tool is in API Schema      │ Shell Tool NOT      │
 │ LLM can output tool call payload │ registered in Schema│
 │ Depends on filter/guardrail     │ Execution impossible│
 └──────────────────────────────────┴─────────────────────┘
```

---

## Complete Architectural Blueprint for DevStack

For DevStack's goal of building an automated, source-code-restricted coding agent integrated with DevStack MCP Server, we recommend the following setup:

### Recommended Setup: OpenCode + DevStack MCP Server (OR Custom AGY / LangGraph Runner)

#### Option 1: OpenCode Integration (Config-Driven)
Place the following `opencode.json` in the DevStack workspace:

```json
{
  "mcpServers": {
    "devstack": {
      "command": "dotnet",
      "args": ["run", "--project", "src/Server/DevStack.Mcp/DevStack.Mcp.csproj"]
    }
  },
  "permission": {
    "bash": "deny",
    "mcp:devstack/*": "allow",
    "read": "allow",
    "edit": "allow"
  }
}
```

#### Option 2: Python Runner (LangGraph / AGY SDK / OpenHands SDK)
Create a Python runner script that connects to `DevStack.Mcp` and loads workspace file tools:

```python
import asyncio
from langchain_community.agent_toolkits import FileManagementToolkit
from langchain_mcp import MCPToolkit
from langgraph.prebuilt import create_react_agent
from langchain_anthropic import ChatAnthropic

async def run_devstack_agent():
    # 1. Load File Tools scoped strictly to ./src
    file_tools = FileManagementToolkit(
        root_dir="./src",
        selected_tools=["read_file", "write_file", "list_directory", "file_search"]
    ).get_tools()

    # 2. Connect to DevStack MCP Server
    mcp_toolkit = MCPToolkit(
        server_command=["dotnet", "run", "--project", "src/Server/DevStack.Mcp/DevStack.Mcp.csproj"]
    )
    mcp_tools = await mcp_toolkit.get_tools()

    # 3. Combine tools (ZERO shell/bash tools included)
    agent_tools = file_tools + mcp_tools

    # 4. Instantiate Agent
    model = ChatAnthropic(model="claude-3-5-sonnet-20241022")
    agent = create_react_agent(model, agent_tools)
    
    # 5. Execute task loop safely
    response = await agent.ainvoke({"messages": [("user", "Fetch the next task from DevStack and implement it in ./src")]})
    print(response)

if __name__ == "__main__":
    asyncio.run(run_devstack_agent())
```

---

## Conclusion

By integrating **MCP Servers** alongside workspace file tools and programmatically excluding shell/terminal tools, DevStack achieves the gold standard of automated AI coding: **full task management capability and code editing power with zero operating system vulnerability.**

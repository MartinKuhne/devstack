# Market Research: Fine-Grained Tool Control & Workspace Sandboxing in AI Coding Agents

## Executive Summary

This research note evaluates current market solutions (frameworks, CLI tools, harness platforms, and SDKs) for building an **automated coding agent** that requires strict access control. 

### Core Requirement
The target agent must be **strictly confined to source code directory operations** (reading, writing, searching, listing, and diffing files within a designated workspace directory) and **completely denied access to the operating system**, shell environments (Bash, PowerShell, zsh), subprocess invocation, or network/system administration utilities.

### Key Finding
Coding agents fall into two major architectural paradigms regarding tool permissions:

1. **Interactive Developer CLI Harnesses** (e.g., *Claude Code*, *Aider*): Designed around developer-in-the-loop workflows where shell execution is a fundamental primitive. Tool controls in these tools rely primarily on user confirmation popups, command pattern allowlists, or system prompts. Disabling shell access entirely is either unsupported or significantly degrades the agent's core capabilities.
2. **Programmatic Agent Frameworks & Mode-Based Harnesses** (e.g., *OpenHands SDK*, *LangGraph*, *OpenCode*, *Pi*, *AGY (Antigravity SDK)*, *Roo Code / Cline Custom Modes*, *AutoGen*, *CrewAI*, *Hermes*): Expose hard structural or configuration-level mechanisms to whitelist specific file-system tools while **programmatically excluding or explicitly denying shell/terminal execution tools**.

### Strategic Recommendation for DevStack
To achieve a production-grade, prompt-injection-proof automated coding agent limited strictly to source files, DevStack should adopt one of the following top-tier options:
- **Primary Option (SDK / Ecosystem Native)**: A custom agent using **OpenHands SDK**, **LangGraph**, or **AGY Python SDK (`google-antigravity`)** configured with a restricted tool capabilities set (`CapabilitiesConfig` / `FileManagementToolkit`) scoped strictly to `./src` with zero terminal/shell tools registered.
- **Secondary Option (Configured Harness)**: **OpenCode** or **Roo Code / Cline CLI** running in headless mode with `"permission": { "bash": "deny" }` in `opencode.json` or a custom `.roomodes` file that completely strips the `command` (`execute_command`) tool group.

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
        tools=[Tool(name=FileEditorTool.name)]
    )
    ```
  - Micro-agents and skills can be globally disabled via `config.toml` (`enable_prompt_extensions = false` or `disabled_microagents = [...]`).
* **OS / Shell Isolation**: Exceptional. When `TerminalTool` / `CmdRunAction` is omitted from `tools`, the LLM function schema contains zero command-execution entries. Prompt injection cannot invoke shell commands because the underlying runtime dispatcher has no shell tool registered.
* **Workspace Confinement**: High. Supports volume mounting and root directory scoping.
* **Fit for DevStack**: **10 / 10** (Ideal for building headless, file-only background subagents).

#### 1.2 LangChain / LangGraph
* **Overview**: Industrial-standard graph-based orchestration framework for stateful AI agents.
* **Tool Control Mechanism**:
  - Provides declarative tool binding via `FileManagementToolkit`.
  - Developers explicitly select allowed tools and enforce directory scoping via `root_dir`:
    ```python
    from langchain_community.agent_toolkits import FileManagementToolkit

    file_toolkit = FileManagementToolkit(
        root_dir="./src",
        selected_tools=["read_file", "write_file", "list_directory", "file_search"]
    )
    tools = file_toolkit.get_tools()
    ```
  - Omits `ShellTool`, `BashProcessTool`, and `PythonAstREPLTool`.
* **OS / Shell Isolation**: Exceptional. Hard structural boundary. The LLM receives function declarations solely for file I/O within `root_dir`.
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
            # Command tools excluded
        )
    )
    async with Agent(config) as agent:
        # Agent execution loop
    ```
* **OS / Shell Isolation**: High. Fine-grained tool action permission enforcement (`read_file`, `write_file`, `command`) at the SDK and CLI runtime layer.
* **Workspace Confinement**: High. Bounded by workspace settings and fine-grained target path permissions.
* **Fit for DevStack**: **9.5 / 10** (Native alignment with AGY ecosystem and Python SDK agent leasing).

#### 1.4 Microsoft AutoGen / AutoGen v0.4 (Magentic-One)
* **Overview**: Multi-agent conversation framework for complex workflows.
* **Tool Control Mechanism**:
  - Execution capability is governed by `code_execution_config` and tool registration.
  - Setting `code_execution_config=False` on `UserProxyAgent` or omitting execution backends (Docker/LocalCommandLine) completely disables code/shell execution.
  - Tools are registered granularly per agent using `@agent.register_for_execution`.
* **OS / Shell Isolation**: Very High. Disabling the execution backend stops all tool call execution requests.
* **Workspace Confinement**: Depends on registered custom file functions.
* **Fit for DevStack**: **9 / 10**.

#### 1.5 CrewAI
* **Overview**: High-level multi-agent orchestration framework centered around role-based agents.
* **Tool Control Mechanism**:
  - Declarative tool assignment per agent: `Agent(role="Code Writer", tools=[FileReadTool(), FileWriterTool()])`.
  - Avoids adding `CodeInterpreterTool` or custom shell execution tools.
* **OS / Shell Isolation**: Very High.
* **Fit for DevStack**: **8.5 / 10**.

---

### 2. Headless CLI Coding Agents & Harnesses (Configuration-Based Control)

#### 2.1 OpenCode
* **Overview**: Open-source CLI agent framework and runtime runner.
* **Tool Control Mechanism**:
  - Configured via `opencode.json` in project root or globally at `~/.config/opencode/opencode.json`, as well as agent frontmatter (`.opencode/agent/<name>.md`).
  - Supports explicit permission rules to block shell access entirely:
    ```json
    {
      "permission": {
        "bash": "deny"
      }
    }
    ```
  - Allows fine-grained command-level pattern allowlists/denylists (`"bash": { "*": "deny", "git status": "allow" }`).
* **OS / Shell Isolation**: High. Setting `"bash": "deny"` in `opencode.json` completely prevents the agent from calling shell execution primitives.
* **Workspace Confinement**: High when restricted via `.opencode/agent` frontmatter and permission settings.
* **Fit for DevStack**: **9.5 / 10** (Strong candidate; `.opencode` configuration directory structure is already established in DevStack).

#### 2.2 Roo Code / Cline (Headless / Custom Modes)
* **Overview**: Popular open-source AI coding assistant supporting VS Code extension and headless CLI execution.
* **Tool Control Mechanism**:
  - Features a **Custom Modes** engine configured via `.roomodes` / `.clinerules` or TOML mode files.
  - Tool access is partitioned into predefined groups: `read`, `edit`, `browser`, `command`, `mcp`.
  - A custom mode can explicitly disable the `command` group (`execute_command`), preventing the agent from invoking terminal commands:
    ```json
    {
      "customModes": [
        {
          "slug": "file-only-coder",
          "name": "Source File Coder",
          "roleDefinition": "You are a code modification agent restricted strictly to file edits.",
          "groups": ["read", "edit"]
        }
      ]
    }
    ```
  - Auto-approval settings allow enabling file edits while permanently locking/denying `execute_command`.
* **OS / Shell Isolation**: High. Stripping the `command` group removes `execute_command` from the active toolset.
* **Workspace Confinement**: Confined to workspace directory opened by the harness.
* **Fit for DevStack**: **9 / 10** (Best option if leveraging an existing IDE/CLI agent harness).

#### 2.3 Pi Agent Harness & Framework
* **Overview**: Lightweight, extensible AI coding harness designed for modular agent workflows.
* **Tool Control Mechanism**:
  - Uses a minimal core architecture, delegating permission control to extension layers (`pi-permission-system`, `pi-permission-layers`, `pi-sandbox`).
  - Can disable bash directly via YAML frontmatter in agent definitions:
    ```yaml
    ---
    name: file-only-agent
    permission:
      tools:
        read: allow
        write: allow
        bash: "off"
    ---
    ```
  - Leverages OS-level sandboxing extensions (`pi-sandbox` using `bubblewrap` or `sandbox-exec`) to strictly bind read/write access paths (`allowRead: ["./src"]`).
* **OS / Shell Isolation**: High (when `bash: "off"` is declared in frontmatter or `pi-permission-system` extension is enabled).
* **Workspace Confinement**: High when combined with `pi-sandbox`.
* **Fit for DevStack**: **9 / 10**.

#### 2.4 Goose (Block / Square)
* **Overview**: Open-source extensible AI agent CLI built on the Model Context Protocol (MCP).
* **Tool Control Mechanism**:
  - Manages capabilities via extensions defined in `config.yaml` and granular permissions in `permission.yaml`.
  - Operating modes include `Chat Only`, `Smart Approval`, `Manual Approval`, and `Autonomous`.
  - Built-in "Developer" extension exposes shell tools. Users can disable the Developer extension and load only custom file-system MCP extensions.
* **OS / Shell Isolation**: High (when developer extension is toggled off or custom MCP profiles are configured).
* **Fit for DevStack**: **8 / 10**.

#### 2.5 Hermes (Nous Research)
* **Overview**: Autonomous AI agent framework built by Nous Research.
* **Tool Control Mechanism**:
  - Configuration managed via `~/.hermes/config.yaml` or `hermes config` CLI.
  - Controls tool capabilities at the toolset category level (`hermes tools disable <toolset>`).
  - Provides a "Dangerous Command Approval" system (`approvals.mode`: `manual`, `smart`, `off`).
  - Supports switching terminal backends (`terminal.backend`: `docker` vs `process`).
* **OS / Shell Isolation**: Moderate to High. Shell execution can be disabled by toggling off the terminal toolset or sandboxed inside Docker (`terminal.backend: docker`). However, Hermes lacks fine-grained per-command regex allowlisting found in OpenCode or Roo Code.
* **Fit for DevStack**: **7.5 / 10**.

#### 2.6 Claude Code (Anthropic CLI)
* **Overview**: Anthropic's official CLI research/development coding agent.
* **Tool Control Mechanism**:
  - Configuration via `~/.claude/settings.json` or project-level `.claude/settings.json`.
  - Provides tool permission matching (e.g., `Bash(git:*)`, `deny: ["Bash"]`).
* **OS / Shell Isolation**: Moderate. While `Bash` can be denied or set to require prompt confirmation, Claude Code's internal agent loop assumes a bash prompt is available for environment inspection (`ls`, `git status`, test execution). Completely denying `Bash` can lead to degraded agent performance or stuck execution loops.
* **Fit for DevStack**: **6 / 10** (Not optimized for shell-less autonomous operation).

#### 2.7 Aider
* **Overview**: Leading CLI AI pair programming tool.
* **Tool Control Mechanism**:
  - Focuses on file manipulation (`/add`, `/read-only`).
  - Lacks native configuration flags to disable terminal execution (`/run`, `/test`) entirely. Aider relies on interactive user prompts before running terminal commands.
* **OS / Shell Isolation**: Low natively. Requires external OS sandboxing (Docker container, macOS Seatbelt, or Linux Landlock/seccomp) to prevent shell execution.
* **Fit for DevStack**: **4 / 10** (Unsuitable for standalone headless shell-less automation without external containerization).

---

## Market Comparison Matrix

| Agent / Framework | Tool Exclusion Granularity | OS/Bash Disabling | Path Confinement (`root_dir`) | Headless Automation | Prompt Injection Safety | Overall Suitability |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **OpenHands SDK** | Fine (Per Tool) | **Programmatic Exclusion** | Supported | Excellent (SDK) | **Highest** (No Tool Schema) | **10 / 10** |
| **LangGraph / LangChain** | Fine (Per Tool) | **Programmatic Exclusion** | Built-in (`root_dir`) | Excellent (SDK) | **Highest** (No Tool Schema) | **10 / 10** |
| **OpenCode** | Fine (Pattern / Flag) | **Config (`bash: "deny"`)** | Frontmatter / Config | Excellent (CLI) | **High** (Config Boundary) | **9.5 / 10** |
| **AGY (`google-antigravity`)** | Fine (Action & Target) | **Programmatic (`CapabilitiesConfig`)**| Built-in Policy | Excellent (SDK / CLI) | **High** (Policy / Schema) | **9.5 / 10** |
| **Roo Code / Cline** | Group & Tool Level | **Config-Based (Remove `command`)** | Workspace Bounded | Good (Headless CLI) | High (Disabled Group) | **9 / 10** |
| **Pi Agent** | Fine (Extension / Frontmatter)| **Frontmatter (`bash: "off"`)** | Via `pi-sandbox` | Good (CLI) | High | **9 / 10** |
| **AutoGen v0.4** | Fine (Per Tool) | **Programmatic (`code_execution=False`)** | Via Custom Handlers | Excellent (SDK) | High | **9 / 10** |
| **CrewAI** | Fine (Per Tool) | **Programmatic Exclusion** | Custom Tools | Good (SDK) | High | **8.5 / 10** |
| **Goose (Block)** | Extension / MCP Level | **Extension Unloading** | Custom MCP | Good (CLI/Desktop) | High | **8 / 10** |
| **Hermes** | Toolset Category Level | **Toolset Disable / Docker** | Container Scoped | Good (CLI) | Moderate | **7.5 / 10** |
| **Claude Code** | Permission Pattern | Soft Deny (`deny: ["Bash"]`) | Project Dir | Moderate (CLI) | Moderate (Agent expects Bash) | **6 / 10** |
| **Aider** | Coarse | Interactive Prompting Only | Git Repository | Moderate | Low (Requires Docker) | **4 / 10** |

---

## Security Architecture: Hard vs. Soft Isolation

When designing an automated coding agent restricted to source files, relying on **soft isolation** (system prompts or user confirmation UI) is insufficient.

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

### Why Programmatic Exclusion is Mandatory
1. **Prompt Injection Resistance**: If a shell tool (`bash`, `powershell`, `execute_command`) is present in the LLM's tool definition list, malicious code inside source files or user inputs can trick the LLM into generating a tool call. If the tool is **never registered** in the API payload sent to the LLM, the LLM physically cannot generate a valid tool call for it.
2. **Deterministic Security Boundary**: Programmatic tool exclusion eliminates reliance on prompt alignment or runtime regex command filtering.

### File System Path Confinement Safeguards
Even when shell tools are removed, file tools (`read_file`, `write_file`) must be secured against workspace escape:
* **Canonical Path Validation**: Resolve all paths using `realpath` / `Path.resolve()` to prevent symlink traversal.
* **Prefix Checking**: Ensure `resolved_path.starts_with(workspace_root)` for every read/write action.
* **Sensitive File Blacklisting**: Exclude hidden security files (`.env`, `.git/config`, SSH keys) even within the workspace.

---

## Architectural Recommendation for DevStack

For DevStack's goal of building an automated, source-code-restricted coding agent, we recommend a **two-tier implementation plan**:

### Phase 1: Native Custom Subagent (Recommended)
Build a dedicated DevStack agent runner in Python or TypeScript using **OpenHands SDK**, **AGY Python SDK (`google-antigravity`)**, or **LangGraph**.
- **Tools Registered**:
  - `read_file(path: string)`
  - `write_file(path: string, content: string)`
  - `list_directory(path: string)`
  - `grep_search(query: string, pattern: string)`
  - `apply_diff(path: string, diff: string)`
- **Tools Excluded**:
  - `bash`, `powershell`, `exec`, `terminal`, `python_repl`.
- **Scope**: Restricted strictly to `C:\Users\mkuhn\src\devstack\src` (or target project source directory).

### Phase 2: OpenCode / Roo Code Harness Integration (Alternative)
If utilizing a pre-built CLI agent runner, integrate **OpenCode** with `"permission": { "bash": "deny" }` in `opencode.json` (or **Roo Code** with custom `.roomodes` excluding the `command` group).

---

## Conclusion

The market research confirms that **OpenHands SDK**, **LangGraph**, **AGY (`google-antigravity`)**, **OpenCode**, and **Pi** provide top-tier fine-grained tool control for creating a safe, file-only coding agent. By programmatically omitting shell tools or enforcing strict permission denials at the configuration/frontmatter layer, DevStack can achieve a zero-trust, automated code modification engine operating strictly within source code boundaries.

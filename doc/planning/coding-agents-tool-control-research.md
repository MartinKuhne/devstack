# Market Research: Fine-Grained Tool Control & Workspace Sandboxing in AI Coding Agents

## Executive Summary

This research note evaluates current market solutions (frameworks, CLI tools, harness platforms, and SDKs) for building an **automated coding agent** that requires strict access control and compatibility with **.NET 10** (matching DevStack's core server architecture in `src/Server`). 

### Core Requirement
The target agent must be **strictly confined to source code directory operations** (reading, writing, searching, listing, and diffing files within a designated workspace directory) and **completely denied access to the operating system**, shell environments (Bash, PowerShell, zsh), subprocess invocation, or network/system administration utilities.

### Key Finding
Coding agents fall into two major architectural paradigms regarding tool permissions:

1. **Interactive Developer CLI Harnesses** (e.g., *Claude Code*, *Aider*): Designed around developer-in-the-loop workflows where shell execution is a fundamental primitive. Tool controls in these tools rely primarily on user confirmation popups, command pattern allowlists, or system prompts. Disabling shell access entirely is either unsupported or significantly degrades the agent's core capabilities.
2. **Programmatic Agent Frameworks & Mode-Based Harnesses** (e.g., *Microsoft Semantic Kernel*, *AutoGen.NET*, *OpenHands SDK*, *LangGraph*, *OpenCode*, *Pi*, *AGY*, *Roo Code / Cline Custom Modes*, *Goose*): Expose hard structural or configuration-level mechanisms to whitelist specific file-system tools and **Model Context Protocol (MCP)** tools while **programmatically excluding or explicitly denying shell/terminal execution tools**.

### Strategic Recommendation for DevStack
To achieve a production-grade, prompt-injection-proof automated coding agent limited strictly to source files while maintaining architectural consistency with DevStack's **.NET 10 / C#** codebase (`src/Server/DevStack.slnx`):

- **Primary Option (Native .NET Stack)**: **Microsoft Semantic Kernel (`Microsoft.SemanticKernel`)** or **AutoGen.NET (`Microsoft.AutoGen.Core`)**. This approach runs 100% natively in C# / .NET 10, references `DevStack.Domain` and `DevStack.Application` directly, integrates with `DevStack.Mcp` via the C# MCP SDK (`ModelContextProtocol.Sdk`), and enforces a strict C# workspace file plugin with zero terminal/shell tools registered.
- **Secondary Option (Configured Out-of-Process CLI Harness)**: **OpenCode** or **AGY CLI** running in headless mode with `"permission": { "bash": "deny", "mcp:devstack/*": "allow" }` in `opencode.json` or AGY settings, invoking `DevStack.Mcp` via Stdio execution (`dotnet run --project src/Server/DevStack.Mcp`).

---

## Detailed Market Survey of Coding Agents & Frameworks

### 1. Native .NET Agent Frameworks & SDKs (Highest Architectural Consistency)

#### 1.1 Microsoft Semantic Kernel (`Microsoft.SemanticKernel`)
* **Overview**: Microsoft's official, open-source AI orchestration framework for C# and .NET.
* **Tool Control Mechanism**:
  - Uses explicit plugin registration (`kernel.Plugins.AddFromType<T>()`).
  - Only registered plugins and kernel functions are exposed in the LLM's function schema.
  - **Shell Isolation**: Terminal/process execution plugins are omitted entirely. The agent receives zero command execution functions.
  - **Workspace Confinement**: A custom `WorkspaceFilePlugin` written in C# enforces path canonicalization (`Path.GetFullPath`) and root directory prefix checks (`path.StartsWith(workspaceRoot)`).
* **MCP Integration**: Native via `Microsoft.SemanticKernel.Plugins` and `ModelContextProtocol.Sdk`. Connects directly to `DevStack.Mcp` in-process or via Stdio.
* **.NET Compatibility**: **100% Native C# / .NET 10**. Shares types directly with `DevStack.Domain` and `DevStack.Application`.
* **Fit for DevStack**: **10 / 10** (Best architectural consistency with `src/Server`).

#### 1.2 Microsoft AutoGen.NET (`Microsoft.AutoGen.Core`)
* **Overview**: Official C# implementation of Microsoft's AutoGen multi-agent framework.
* **Tool Control Mechanism**:
  - Execution capability is governed by `ICodeExecutor`. Setting code execution handlers to `null` or omitting `LocalCommandLineExecutor` permanently disables command execution.
* **MCP Integration**: Compatible via .NET MCP client wrappers.
* **.NET Compatibility**: Native C# / .NET 10.
* **Fit for DevStack**: **9.5 / 10**.

---

### 2. Open-Source Multi-Language Agent Frameworks & SDKs

#### 2.1 OpenHands SDK (formerly OpenDevin)
* **Overview**: Modular agent framework and evaluation platform designed for autonomous software engineering.
* **Tool Control Mechanism**:
  - OpenHands provides explicit tool injection when instantiating agents via its Python SDK (`openhands.sdk.Agent`).
  - `TerminalTool` / `CmdRunAction` is explicitly omitted from the `tools` array.
* **OS / Shell Isolation**: Exceptional. Programmatic exclusion.
* **MCP Integration**: Fully supported via SDK MCP client wrappers.
* **.NET Compatibility**: Requires a Python sidecar process to run the agent, interacting with .NET over MCP Stdio/HTTP.
* **Fit for DevStack**: **7.5 / 10** (Requires Python runtime alongside .NET backend).

#### 2.2 LangChain / LangGraph
* **Overview**: Industrial-standard graph-based orchestration framework for stateful AI agents.
* **Tool Control Mechanism**:
  - Provides declarative tool binding via `FileManagementToolkit` and `MCPToolkit`.
  - `ShellTool` / `BashProcessTool` is completely omitted.
* **OS / Shell Isolation**: Exceptional. Hard structural boundary.
* **.NET Compatibility**: Python / TypeScript stack; requires IPC sidecar to interface with DevStack .NET server.
* **Fit for DevStack**: **7.5 / 10**.

#### 2.3 Antigravity CLI & SDK (`agy`)
* **Overview**: Google Antigravity AI-first development CLI (`agy`) and Python SDK (`google-antigravity`).
* **Tool Control Mechanism**:
  - In CLI mode (`agy`), uses structured permission management (`settings.json`).
  - In Python SDK mode (`google.antigravity`), tool capabilities are configured programmatically via `CapabilitiesConfig` (command tools excluded, MCP tools explicitly granted).
* **MCP Integration**: Native support for `mcpServers` (including .NET MCP servers).
* **.NET Compatibility**: Operates as a CLI or Python SDK; integrates with DevStack .NET server over Stdio/JSON-RPC.
* **Fit for DevStack**: **9.5 / 10** (Native alignment with AGY ecosystem).

---

### 3. Headless CLI Coding Agents & Harnesses (Configuration-Based Control)

#### 3.1 OpenCode
* **Overview**: Open-source CLI agent framework and runtime runner.
* **Tool Control Mechanism**:
  - Configured via `opencode.json` in project root or `.opencode/agent/<name>.md`.
  - Blocks shell access while allowing DevStack .NET MCP server:
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
* **OS / Shell Isolation**: High (`"bash": "deny"`).
* **.NET Compatibility**: Excellent interop over MCP Stdio (`dotnet run`). `.opencode` directory structure is already present in DevStack.
* **Fit for DevStack**: **9.5 / 10**.

#### 3.2 Roo Code / Cline (Headless / Custom Modes)
* **Overview**: Popular open-source AI coding assistant supporting VS Code extension and headless CLI execution.
* **Tool Control Mechanism**:
  - Custom modes in `.roomodes` strip the `command` group (`execute_command`) while enabling `mcp`, `read`, and `edit`.
* **.NET Compatibility**: High interop via `mcpSettings.json` running `dotnet run --project ...`.
* **Fit for DevStack**: **9 / 10**.

#### 3.3 Pi Agent Harness & Framework
* **Overview**: Modular AI coding harness with extension permissions (`pi-permission-system`, `pi-sandbox`).
* **Tool Control Mechanism**: Disables bash (`bash: "off"`) via YAML frontmatter; binds MCP tools via `pi-extension-mcp`.
* **.NET Compatibility**: Good interop over MCP Stdio.
* **Fit for DevStack**: **9 / 10**.

#### 3.4 Goose (Block)
* **Overview**: 100% MCP-native AI agent CLI.
* **Tool Control Mechanism**: Unloads built-in "Developer" CLI extension; loads DevStack .NET MCP server extension.
* **.NET Compatibility**: High interop via MCP Stdio.
* **Fit for DevStack**: **8.5 / 10**.

---

## .NET Ecosystem Compatibility & Native C# Framework Assessment

Building the restricted agent natively in **.NET 10** offers substantial advantages for DevStack:
1. **Zero Runtime Overhead**: No need for Python/Node sidecar processes or containerized inter-language RPC.
2. **Direct Entity Sharing**: C# agents can directly reference `DevStack.Domain` entities (Project, Deliverable, AgentTask) and `DevStack.Application` CQRS handlers.
3. **In-Process MCP Hosting**: The agent can consume `DevStack.Mcp` tools in-process without needing Stdio subprocess spawning.

### Native C# Implementation Pattern (Microsoft Semantic Kernel)

Below is a complete native C# implementation blueprint demonstrating a restricted, file-only, MCP-enabled agent built inside DevStack:

```csharp
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace DevStack.Infrastructure.Agents;

public class WorkspaceFilePlugin
{
    private readonly string _workspaceRoot;

    public WorkspaceFilePlugin(string workspaceRoot)
    {
        _workspaceRoot = Path.GetFullPath(workspaceRoot);
    }

    [KernelFunction, System.ComponentModel.Description("Reads a source code file safely within workspace bounds.")]
    public async Task<string> ReadFileAsync(string relativePath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(_workspaceRoot, relativePath));
        if (!fullPath.StartsWith(_workspaceRoot, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Path traversal outside workspace is forbidden.");

        return await File.ReadAllTextAsync(fullPath);
    }

    [KernelFunction, System.ComponentModel.Description("Writes content to a source code file within workspace bounds.")]
    public async Task WriteFileAsync(string relativePath, string content)
    {
        string fullPath = Path.GetFullPath(Path.Combine(_workspaceRoot, relativePath));
        if (!fullPath.StartsWith(_workspaceRoot, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Path traversal outside workspace is forbidden.");

        await File.WriteAllTextAsync(fullPath, content);
    }
}
```

---

## Comprehensive Market Comparison Matrix

| Agent / Framework | Native Language | .NET 10 Compatibility | Tool Exclusion Mechanism | MCP Integration | Path Confinement | Suitability |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Microsoft Semantic Kernel** | **C# / .NET 10** | **100% Native (In-Process)** | **Programmatic Plugin Whitelist** | Native (`ModelContextProtocol.Sdk`) | Custom C# Plugin (`StartsWith`) | **10 / 10** |
| **AutoGen.NET** | **C# / .NET 10** | **100% Native (In-Process)** | **Programmatic (`ICodeExecutor=null`)** | Native C# MCP | Custom C# Handlers | **9.5 / 10** |
| **OpenCode** | Go / CLI | High (MCP Stdio Interop) | **Config (`bash: "deny"`)** | Native (`mcpServers`) | Frontmatter / Config | **9.5 / 10** |
| **AGY (`google-antigravity`)** | Python / CLI | High (MCP Stdio Interop) | **Policy (`CapabilitiesConfig`)** | Native (`mcpServers`) | Built-in Policy | **9.5 / 10** |
| **Roo Code / Cline** | TS / CLI | High (MCP Stdio Interop) | **Config (Remove `command` group)** | Native (`mcpSettings`) | Workspace Bounded | **9.0 / 10** |
| **Pi Agent** | TS / CLI | High (MCP Stdio Interop) | **Frontmatter (`bash: "off"`)** | Extension (`pi-extension-mcp`) | Via `pi-sandbox` | **9.0 / 10** |
| **Goose (Block)** | Rust / CLI | High (MCP Stdio Interop) | **Extension Unloading** | 100% MCP Native | Custom MCP | **8.5 / 10** |
| **OpenHands SDK** | Python | Requires Python Sidecar | **Programmatic Exclusion** | Native (`Tool.from_mcp`) | Supported | **7.5 / 10** |
| **LangGraph** | Python / TS | Requires Python Sidecar | **Programmatic Exclusion** | Native (`MCPToolkit`) | Built-in (`root_dir`) | **7.5 / 10** |
| **Claude Code** | TS / CLI | High (MCP Stdio Interop) | Soft Deny (`deny: ["Bash"]`) | Via Config | Project Dir | **6.0 / 10** |

---

## Conclusion & Architectural Recommendation for DevStack

To maximize architectural consistency with DevStack's C# / .NET 10 codebase (`src/Server`):

1. **Top Native .NET Choice**: **Microsoft Semantic Kernel (`Microsoft.SemanticKernel`)**
   - Integrates directly into `DevStack.Infrastructure` or `DevStack.Api`.
   - Programmatically registers only `WorkspaceFilePlugin` and `DevStack.Mcp` endpoints, ensuring 100% shell-less execution with zero Python/Node sidecar overhead.

2. **Top Out-of-Process CLI Choice**: **OpenCode** or **AGY CLI**
   - Best if an external CLI runner is preferred. Connects to `DevStack.Mcp` via Stdio while setting `"bash": "deny"`.

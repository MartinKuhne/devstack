In its current state, the easiest way to try this out is to have it modify itself.

# Warning

- This is primarily a research project. YMMV.
- There is no authentication in this iteration. If someone creates an task to sell your cat on the internet, it probably will.

# Prerequisites

- Docker
- Clone this repository

# Installation

Create an .env file. Using secrets 'abc' and 'def' in this example

```
DEVSTACK_SECRET_KEY=abc
ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=devstack;Username=devstack;Password=def"
POSTGRES_PASSWORD=def
```

Run

```
docker compose up --build -d
```

Then, head to http://localhost:8087 for the admin UI.

The DevStack MCP server is available at `http://localhost:8088/mcp`. Configure it in your AI coding tool of choice:

## MCP Server Configuration

### OpenCode

Add to `opencode.json` in the repository root:

```json
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "devstack": {
      "type": "remote",
      "url": "http://localhost:8088/mcp",
      "enabled": true
    }
  }
}
```

### Claude Desktop

Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "devstack": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-remote", "--url", "http://localhost:8088/mcp"]
    }
  }
}
```

Alternatively, if using Claude Code CLI:

```bash
claude mcp add devstack -- npx -y @modelcontextprotocol/server-remote --url http://localhost:8088/mcp
```

### OpenAI Codex CLI

Add to `~/.codex/config.toml`:

```toml
[mcp_servers.devstack]
url = "http://localhost:8088/mcp"
```

Or via the CLI:

```bash
codex mcp add devstack --url http://localhost:8088/mcp
```

### Google Antigravity

Add to `~/.gemini/config/mcp_config.json`:

```json
{
  "mcpServers": {
    "devstack": {
      "serverUrl": "http://localhost:8088/mcp"
    }
  }
}
```

### Pi (MCP Adapter)

Add to `~/.pi/agent/mcp.json`:

```json
{
  "mcpServers": {
    "devstack": {
      "url": "http://localhost:8088/mcp"
    }
  }
}
```


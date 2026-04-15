# MCP Tools Reference

This document describes all Model Context Protocol (MCP) servers configured in your opencode setup.

## Table of Contents

- [Filesystem](#filesystem)
- [Git](#git)
- [Memory](#memory)
- [Context7](#context7)
- [NuGet](#nuget)
- [DotNet](#dotnet)
- [Saga](#saga)
- [Refactor](#refactor)
- [Fetch](#fetch)

## Usage Recommendations

### For Development Work
1. **DotNet** - Build, test, run .NET projects
2. **Git** - Version control operations
3. **Filesystem** - Read/write code files
4. **NuGet** - Package management
5. **Refactor** - Code transformations

### For Documentation & Research
1. **Context7** - Library documentation
2. **Fetch** - Web content
3. **Codebase-Memory** / **Serena** - Codebase understanding

### For Project Management
1. **Saga** - Task and project tracking
2. **Memory** - Persistent notes and context

### For Code Quality
1. **DotNet** - Build and test
2. **Refactor** - Automated refactoring
3. **Git** - Review changes

---

## Filesystem

Provides secure file and directory operations within allowed directories.

### Tools

| Tool | Description |
|------|-------------|
| `read_text_file` | Read file contents as text (supports head/tail for partial reads) |
| `read_media_file` | Read image/audio files and return base64 data |
| `read_multiple_files` | Read multiple files simultaneously |
| `write_file` | Create or overwrite files |
| `edit_file` | Pattern-based file edits with dry-run preview |
| `create_directory` | Create directories (including parents) |
| `list_directory` | List directory contents |
| `list_directory_with_sizes` | List with file sizes and summary stats |
| `move_file` | Move or rename files/directories |
| `search_files` | Glob-style recursive file search |
| `directory_tree` | Get recursive JSON directory structure |
| `get_file_info` | Get file/directory metadata |
| `list_allowed_directories` | List accessible directories |

### Key Features
- Access restricted to configured directories
- Dry-run mode for safe edits
- Simultaneous multi-file reads
- Metadata extraction (size, timestamps, permissions)

---

## Git

Git repository interaction and automation.

### Tools

| Tool | Description |
|------|-------------|
| `git_status` | Show working tree status |
| `git_diff_unstaged` | Show unstaged changes |
| `git_diff_staged` | Show staged changes |
| `git_diff` | Compare branches or commits |
| `git_commit` | Create commit with message |
| `git_add` | Stage files |
| `git_reset` | Unstage all changes |
| `git_log` | Show commit history (with date filtering) |
| `git_create_branch` | Create new branch |
| `git_checkout` | Switch branches |
| `git_show` | Show commit contents |
| `git_branch` | List branches (local/remote/all) |

### Key Features
- Date-based commit filtering (ISO 8601, relative dates)
- Configurable context lines for diffs
- Branch filtering by commit containment

---

## Memory

Persistent knowledge graph for cross-session memory.

### Concepts

- **Entities**: Named nodes with type and observations (e.g., "John_Smith", type: "person")
- **Relations**: Directed connections between entities in active voice (e.g., "works_at")
- **Observations**: Atomic facts attached to entities

### Tools

| Tool | Description |
|------|-------------|
| `create_entities` | Create new entities |
| `create_relations` | Create relations between entities |
| `add_observations` | Add facts to entities |
| `delete_entities` | Remove entities and relations |
| `delete_observations` | Remove specific facts |
| `delete_relations` | Remove relations |
| `read_graph` | Read entire knowledge graph |
| `search_nodes` | Search by query |
| `open_nodes` | Retrieve specific nodes by name |

### Use Cases
- Store user preferences and identity
- Track project decisions and context
- Remember relationships between entities
- Persistent notes across sessions

---

## Context7

Documentation and code examples resolver.

### Tools

| Tool | Description |
|------|-------------|
| `resolve-library-id` | Resolve package name to Context7 library ID |
| `query-docs` | Query documentation and get code examples |

### Key Features
- Up-to-date library documentation
- Code examples for specific use cases
- Version-specific documentation
- Source reputation and benchmark scores

### Use Cases
- Get reference implementation for libraries
- Find code examples for patterns
- Resolve package versions and compatibility

---

## NuGet

NuGet package management and dependency resolution.

### Tools

| Tool | Description |
|------|-------------|
| `get-latest-package-version` | Get latest version with publish date |
| `get-package-context` | Get package documentation (llms.txt or README) |
| `update-package-to-version` | Update packages to specific versions |
| `get-nuget-solver` | Fix vulnerable packages |
| `get-nuget-solver-latest-versions` | Update to latest compatible versions |

### Key Features
- Dependency conflict resolution via NuGetSolver
- Vulnerability detection and fixing
- Prerelease package support
- Package documentation retrieval

---

## DotNet

.NET SDK operations and project management.

### Project Tools

| Tool | Description |
|------|-------------|
| `New` | Create new projects from templates |
| `Restore` | Restore NuGet packages |
| `Build` | Build projects |
| `Run` | Run projects |
| `Test` | Run tests |
| `Publish` | Publish projects |
| `Clean` | Clean build artifacts |
| `Analyze` | Analyze project structure |
| `Pack` | Create NuGet packages |
| `Watch` | Watch mode for development |
| `Format` | Format code |

### Additional Tools

| Tool | Description |
|------|-------------|
| `EF` | Entity Framework Core operations (migrations, scaffolding) |
| `Package` | NuGet package management |
| `Tool` | .NET tool management |
| `Workload` | .NET workload management |
| `SDK` | SDK/runtime/template queries |
| `DevCerts` | Developer certificate and secrets management |
| `Help` | Get command help |
| `Solution` | Solution file management |

### Key Features
- Full .NET CLI coverage
- EF Core migrations support
- Test execution with coverage
- Code formatting and analysis

---

## Saga

Task and project management system.

### Project Tools

| Tool | Description |
|------|-------------|
| `saga_project_create` | Create new project |
| `saga_project_list` | List projects with stats |
| `saga_project_update` | Update project |
| `saga_tracker_dashboard` | Project overview with epics/tasks |
| `saga_tracker_init` | Initialize tracker for project |
| `saga_tracker_search` | Search across all entities |
| `saga_tracker_export` | Export project as JSON |
| `saga_tracker_import` | Import project from JSON |
| `saga_tracker_session_diff` | Show changes since timestamp |

### Epic Tools

| Tool | Description |
|------|-------------|
| `saga_epic_create` | Create epic within project |
| `saga_epic_list` | List epics with task counts |
| `saga_epic_update` | Update epic |

### Task Tools

| Tool | Description |
|------|-------------|
| `saga_task_create` | Create task within epic |
| `saga_task_list` | List tasks with filters |
| `saga_task_get` | Get task details |
| `saga_task_update` | Update task |
| `saga_task_batch_update` | Update multiple tasks |

### Subtask Tools

| Tool | Description |
|------|-------------|
| `saga_subtask_create` | Create subtasks (checklist items) |
| `saga_subtask_update` | Update subtask |
| `saga_subtask_delete` | Delete subtasks |

### Note Tools

| Tool | Description |
|------|-------------|
| `saga_note_save` | Create/update notes |
| `saga_note_list` | List notes with filters |
| `saga_note_search` | Search notes |
| `saga_note_delete` | Delete note |

### Comment Tools

| Tool | Description |
|------|-------------|
| `saga_comment_add` | Add comment to task |
| `saga_comment_list` | List task comments |

### Template Tools

| Tool | Description |
|------|-------------|
| `saga_template_create` | Create reusable task template |
| `saga_template_list` | List templates |
| `saga_template_apply` | Apply template to epic |
| `saga_template_delete` | Delete template |

### Activity Tools

| Tool | Description |
|------|-------------|
| `saga_activity_log` | View activity log |

### Key Features
- Projects → Epics → Tasks → Subtasks hierarchy
- Task dependencies and source code links
- Activity logging and session diffs
- Templates for recurring workflows
- Notes for decisions and context

---

## Refactor

Code refactoring with regex pattern replacement.

### Tools

| Tool | Description |
|------|-------------|
| `refactor_code_refactor` | Refactor code using regex search/replace |
| `refactor_code_search` | Search code patterns with regex |

### Key Features
- Regex-based search and replace
- Context pattern filtering
- Capture group support
- File pattern scoping
- Preview matched text

---

## Fetch

**Status:** Enabled  
**Type:** Local  
**Command:** `uvx mcp-server-fetch`

Web content fetching and extraction.

### Tools

| Tool | Description |
|------|-------------|
| `fetch_fetch` | Fetch URL and extract content as markdown |

### Key Features
- Markdown extraction from HTML
- Configurable content length limits
- Raw HTML option
- Start index for pagination

---

## Codebase-Memory

Custom codebase indexing and knowledge graph.

### Notes
- Custom local installation
- Indexes repository structure
- Provides architecture and dependency insights
- Use for understanding codebase relationships

---

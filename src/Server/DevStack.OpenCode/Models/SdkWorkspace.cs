namespace DevStack.OpenCode.Models;

/// <summary>Project descriptor returned by the server.</summary>
public sealed record SdkProject
{
    /// <summary>Project identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Working tree path.</summary>
    [JsonPropertyName("worktree")] public string Worktree { get; init; } = string.Empty;
    /// <summary>VCS directory (e.g. <c>.git</c>).</summary>
    [JsonPropertyName("vcsDir")] public string? VcsDir { get; init; }
    /// <summary>Version control system, if any.</summary>
    [JsonPropertyName("vcs")] public string? Vcs { get; init; }
    /// <summary>Timing metadata.</summary>
    [JsonPropertyName("time")] public ProjectTime Time { get; init; } = new();
}

/// <summary>Timing metadata for a project.</summary>
public sealed record ProjectTime
{
    /// <summary>Epoch milliseconds when the project was created.</summary>
    [JsonPropertyName("created")] public long Created { get; init; }
    /// <summary>Epoch milliseconds when the project was initialized.</summary>
    [JsonPropertyName("initialized")] public long? Initialized { get; init; }
}

/// <summary>Server path descriptor from <c>GET /path</c>.</summary>
public sealed record ServerPath
{
    /// <summary>Current server state.</summary>
    [JsonPropertyName("state")] public string State { get; init; } = string.Empty;
    /// <summary>Path to the active config file.</summary>
    [JsonPropertyName("config")] public string Config { get; init; } = string.Empty;
    /// <summary>Worktree path.</summary>
    [JsonPropertyName("worktree")] public string Worktree { get; init; } = string.Empty;
    /// <summary>Current directory.</summary>
    [JsonPropertyName("directory")] public string Directory { get; init; } = string.Empty;
}

/// <summary>VCS info from <c>GET /vcs</c>.</summary>
public sealed record SdkVcsInfo
{
    /// <summary>Current branch name.</summary>
    [JsonPropertyName("branch")] public string Branch { get; init; } = string.Empty;
}

/// <summary>PTY session descriptor.</summary>
public sealed record Pty
{
    /// <summary>Unique PTY id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Human-readable title.</summary>
    [JsonPropertyName("title")] public string Title { get; init; } = string.Empty;
    /// <summary>Command line.</summary>
    [JsonPropertyName("command")] public string Command { get; init; } = string.Empty;
    /// <summary>Command arguments.</summary>
    [JsonPropertyName("args")] public IReadOnlyList<string> Args { get; init; } = Array.Empty<string>();
    /// <summary>Working directory.</summary>
    [JsonPropertyName("cwd")] public string Cwd { get; init; } = string.Empty;
    /// <summary>Status — <c>running</c> or <c>exited</c>.</summary>
    [JsonPropertyName("status")] public string Status { get; init; } = "running";
    /// <summary>Process id.</summary>
    [JsonPropertyName("pid")] public int Pid { get; init; }
}

/// <summary>File or directory node from <c>GET /file</c>.</summary>
public sealed record FileNode
{
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    /// <summary>Path relative to the listing root.</summary>
    [JsonPropertyName("path")] public string Path { get; init; } = string.Empty;
    /// <summary>Absolute filesystem path.</summary>
    [JsonPropertyName("absolute")] public string Absolute { get; init; } = string.Empty;
    /// <summary>Node type — <c>file</c> or <c>directory</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "file";
    /// <summary>True when this node is ignored by watcher rules.</summary>
    [JsonPropertyName("ignored")] public bool Ignored { get; init; }
}

/// <summary>File content descriptor from <c>GET /file/content</c>.</summary>
public sealed record FileContent
{
    /// <summary>Content type — <c>text</c> or <c>binary</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "text";
    /// <summary>File contents.</summary>
    [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;
    /// <summary>Optional diff representation.</summary>
    [JsonPropertyName("diff")] public string? Diff { get; init; }
    /// <summary>Optional patch descriptor.</summary>
    [JsonPropertyName("patch")] public FilePatch? Patch { get; init; }
    /// <summary>Optional encoding — only <c>base64</c> for binary files.</summary>
    [JsonPropertyName("encoding")] public string? Encoding { get; init; }
    /// <summary>Detected mime type.</summary>
    [JsonPropertyName("mimeType")] public string? MimeType { get; init; }
}

/// <summary>Patch descriptor for a file.</summary>
public sealed record FilePatch
{
    /// <summary>Old filename.</summary>
    [JsonPropertyName("oldFileName")] public string OldFileName { get; init; } = string.Empty;
    /// <summary>New filename.</summary>
    [JsonPropertyName("newFileName")] public string NewFileName { get; init; } = string.Empty;
    /// <summary>Old file header.</summary>
    [JsonPropertyName("oldHeader")] public string? OldHeader { get; init; }
    /// <summary>New file header.</summary>
    [JsonPropertyName("newHeader")] public string? NewHeader { get; init; }
    /// <summary>Patch hunks.</summary>
    [JsonPropertyName("hunks")] public IReadOnlyList<FilePatchHunk> Hunks { get; init; } = Array.Empty<FilePatchHunk>();
    /// <summary>Optional index hash.</summary>
    [JsonPropertyName("index")] public string? Index { get; init; }
}

/// <summary>Patch hunk.</summary>
public sealed record FilePatchHunk
{
    /// <summary>Start line in the old file.</summary>
    [JsonPropertyName("oldStart")] public int OldStart { get; init; }
    /// <summary>Line count in the old file.</summary>
    [JsonPropertyName("oldLines")] public int OldLines { get; init; }
    /// <summary>Start line in the new file.</summary>
    [JsonPropertyName("newStart")] public int NewStart { get; init; }
    /// <summary>Line count in the new file.</summary>
    [JsonPropertyName("newLines")] public int NewLines { get; init; }
    /// <summary>Hunk lines (with +/-/space prefix).</summary>
    [JsonPropertyName("lines")] public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}

/// <summary>Tracked file status from <c>GET /file/status</c>.</summary>
public sealed record SdkFile
{
    /// <summary>Path of the file.</summary>
    [JsonPropertyName("path")] public string Path { get; init; } = string.Empty;
    /// <summary>Lines added.</summary>
    [JsonPropertyName("added")] public int Added { get; init; }
    /// <summary>Lines removed.</summary>
    [JsonPropertyName("removed")] public int Removed { get; init; }
    /// <summary>Status — <c>added</c>, <c>deleted</c>, or <c>modified</c>.</summary>
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;
}

/// <summary>Range descriptor (line/character positions).</summary>
public sealed record SdkRange
{
    /// <summary>Start position.</summary>
    [JsonPropertyName("start")] public Position Start { get; init; } = new();
    /// <summary>End position.</summary>
    [JsonPropertyName("end")] public Position End { get; init; } = new();
}

/// <summary>Position inside a file.</summary>
public sealed record Position
{
    /// <summary>Zero-based line number.</summary>
    [JsonPropertyName("line")] public int Line { get; init; }
    /// <summary>Zero-based character offset.</summary>
    [JsonPropertyName("character")] public int Character { get; init; }
}

/// <summary>Workspace symbol descriptor.</summary>
public sealed record Symbol
{
    /// <summary>Symbol name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    /// <summary>LSP symbol kind.</summary>
    [JsonPropertyName("kind")] public int Kind { get; init; }
    /// <summary>Source location.</summary>
    [JsonPropertyName("location")] public SymbolLocation Location { get; init; } = new();
}

/// <summary>Source location for a symbol.</summary>
public sealed record SymbolLocation
{
    /// <summary>Document URI.</summary>
    [JsonPropertyName("uri")] public string Uri { get; init; } = string.Empty;
    /// <summary>Range inside the document.</summary>
    [JsonPropertyName("range")] public SdkRange Range { get; init; } = new();
}

/// <summary>Single text match from <c>GET /find</c>.</summary>
public sealed record TextMatch
{
    /// <summary>Path of the matching file (text payload).</summary>
    [JsonPropertyName("path")] public TextMatchField Path { get; init; } = new();
    /// <summary>Lines around the match (text payload).</summary>
    [JsonPropertyName("lines")] public TextMatchField Lines { get; init; } = new();
    /// <summary>Line number of the match.</summary>
    [JsonPropertyName("line_number")] public int LineNumber { get; init; }
    /// <summary>Absolute offset in the file.</summary>
    [JsonPropertyName("absolute_offset")] public int AbsoluteOffset { get; init; }
    /// <summary>Per-character submatches within the line.</summary>
    [JsonPropertyName("submatches")] public IReadOnlyList<TextSubmatch> Submatches { get; init; } = Array.Empty<TextSubmatch>();
}

/// <summary>Text payload for a match field.</summary>
public sealed record TextMatchField
{
    /// <summary>Text content.</summary>
    [JsonPropertyName("text")] public string Text { get; init; } = string.Empty;
}

/// <summary>Submatch descriptor within a text match.</summary>
public sealed record TextSubmatch
{
    /// <summary>Matched text.</summary>
    [JsonPropertyName("match")] public TextMatchField Match { get; init; } = new();
    /// <summary>Start offset within the line.</summary>
    [JsonPropertyName("start")] public int Start { get; init; }
    /// <summary>End offset within the line.</summary>
    [JsonPropertyName("end")] public int End { get; init; }
}

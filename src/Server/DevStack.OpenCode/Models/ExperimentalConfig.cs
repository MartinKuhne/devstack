namespace DevStack.OpenCode.Models;

/// <summary>Experimental features toggle.</summary>
public sealed record ExperimentalConfig
{
    /// <summary>Disable the paste-summary behavior.</summary>
    [JsonPropertyName("disable_paste_summary")]
    public bool? DisablePasteSummary { get; init; }

    /// <summary>Enable the batch tool.</summary>
    [JsonPropertyName("batch_tool")]
    public bool? BatchTool { get; init; }

    /// <summary>Enable OpenTelemetry spans for AI SDK calls.</summary>
    [JsonPropertyName("openTelemetry")]
    public bool? OpenTelemetry { get; init; }

    /// <summary>Tools that should only be available to primary agents.</summary>
    [JsonPropertyName("primary_tools")]
    public IReadOnlyList<string>? PrimaryTools { get; init; }

    /// <summary>Continue the agent loop when a tool call is denied.</summary>
    [JsonPropertyName("continue_loop_on_deny")]
    public bool? ContinueLoopOnDeny { get; init; }

    /// <summary>Timeout in milliseconds for MCP requests.</summary>
    [JsonPropertyName("mcp_timeout")]
    public int? McpTimeout { get; init; }

    /// <summary>Policy statements applied to supported resources.</summary>
    [JsonPropertyName("policies")]
    public IReadOnlyList<PolicyConfig>? Policies { get; init; }
}

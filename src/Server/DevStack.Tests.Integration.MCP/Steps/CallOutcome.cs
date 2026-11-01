using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Captures the outcome of a <c>tools/call</c> request: either a <see cref="CallToolResult"/>
/// or the protocol exception thrown by the server.
/// </summary>
/// <remarks>
/// The MCP SDK surfaces the two server error styles differently: a tool that throws
/// <c>McpProtocolException</c> produces a JSON-RPC error, which the client re-throws, while any
/// other exception produces a result with <see cref="CallToolResult.IsError"/> set to <c>true</c>.
/// </remarks>
internal sealed record CallOutcome(CallToolResult? Result, Exception? Error)
{
    /// <summary>The successful or error tool result; throws when the server answered with a protocol error instead.</summary>
    public CallToolResult SuccessResult =>
        Result ?? throw new InvalidOperationException($"Expected a tool result but the call failed with: {Error?.Message}");

    /// <summary>The protocol error; throws when the call produced a tool result instead.</summary>
    public Exception ProtocolError =>
        Error ?? throw new InvalidOperationException($"Expected a protocol error but the call returned: {Result?.IsError}");
}

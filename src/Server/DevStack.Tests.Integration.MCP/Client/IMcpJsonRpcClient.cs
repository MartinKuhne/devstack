using System.Threading;

namespace DevStack.Tests.Integration.MCP.Client;

public interface IMcpJsonRpcClient
{
    Task<JsonRpcResponse> SendRequestAsync<TParams>(string method, TParams? @params, CancellationToken cancellationToken = default);
    Task<JsonRpcResponse> SendRequestAsync(string method, CancellationToken cancellationToken = default);
    Task SendNotificationAsync<TParams>(string method, TParams? @params, CancellationToken cancellationToken = default);
    Task<JsonRpcResponse[]> SendBatchRequestAsync(JsonRpcRequest[] requests, CancellationToken cancellationToken = default);
}

namespace DevStack.OpenCode.Client;

/// <summary>
/// Thrown when the OpenCode server returns a non-success status. Carries the
/// parsed <c>data.message</c> and <c>ref</c> fields from the server's error
/// envelope so the caller can see the actual reason (and quote the ref id
/// when grepping server logs).
/// </summary>
public sealed class OpenCodeRequestException : HttpRequestException
{
    /// <summary>Absolute URL of the failed request.</summary>
    public Uri RequestUri { get; }

    /// <summary>
    /// Server-side error reference (the <c>ref</c> field in the body), or
    /// <c>null</c> when the body could not be parsed.
    /// </summary>
    public string? ErrorRef { get; }

    /// <summary>
    /// Server-side error message (the <c>data.message</c> field), or
    /// <c>null</c> when the body could not be parsed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Raw response body, useful when the error envelope could not be
    /// parsed.
    /// </summary>
    public string RawBody { get; }

    /// <summary>Creates the exception from a request + response pair.</summary>
    public OpenCodeRequestException(Uri requestUri, int statusCode, string rawBody, string? errorMessage, string? errorRef, string message)
        : base(message, inner: null, statusCode: (System.Net.HttpStatusCode)statusCode)
    {
        RequestUri = requestUri;
        RawBody = rawBody;
        ErrorMessage = errorMessage;
        ErrorRef = errorRef;
    }
}

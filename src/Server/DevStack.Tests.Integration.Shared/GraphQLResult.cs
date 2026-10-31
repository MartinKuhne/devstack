using System.Net;
using System.Text.Json;

namespace DevStack.Tests.Integration.Shared;

/// <summary>
/// Assertion-friendly view of a GraphQL HTTP response, exposing the data/errors
/// contract that the AdminUi and other clients consume.
/// </summary>
public sealed class GraphQLResult
{
    private GraphQLResult(HttpStatusCode statusCode, string rawBody, JsonElement body)
    {
        StatusCode = statusCode;
        RawBody = rawBody;
        Body = body;
    }

    /// <summary>HTTP status code returned by the endpoint.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Raw response payload, useful for assertion failure messages.</summary>
    public string RawBody { get; }

    /// <summary>Parsed JSON body of the response.</summary>
    public JsonElement Body { get; }

    /// <summary>True when the response carries a non-empty top-level errors array.</summary>
    public bool HasErrors => ErrorCount > 0;

    /// <summary>Number of entries in the top-level errors array.</summary>
    public int ErrorCount => ErrorsOrEmpty.ValueKind == JsonValueKind.Array ? ErrorsOrEmpty.GetArrayLength() : 0;

    /// <summary>The errors array element, or a JSON null element when absent.</summary>
    public JsonElement ErrorsOrEmpty =>
        Body.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array
            ? errors
            : JsonDocument.Parse("null").RootElement;

    /// <summary>True when a non-null data element is present.</summary>
    public bool HasData =>
        Body.TryGetProperty("data", out var data) && data.ValueKind != JsonValueKind.Null;

    /// <summary>True when the data element is explicitly null (resolver-level failure).</summary>
    public bool DataIsNull =>
        Body.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Null;

    /// <summary>The data element. Throws when the response has no data.</summary>
    public JsonElement Data =>
        HasData
            ? Body.GetProperty("data")
            : throw new InvalidOperationException($"GraphQL response has no data: {RawBody}");

    /// <summary>Message of the first error, or an empty string when there are none.</summary>
    public string FirstErrorMessage =>
        HasErrors ? ErrorsOrEmpty[0].TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty : string.Empty;

    /// <summary>extensions.code of the first error, or null when not reported.</summary>
    public string? FirstErrorCode =>
        HasErrors
        && ErrorsOrEmpty[0].TryGetProperty("extensions", out var ext)
        && ext.TryGetProperty("code", out var code)
            ? code.GetString()
            : null;

    /// <summary>True when any error message contains the given text (case-insensitive).</summary>
    public bool AnyErrorContains(string text)
    {
        if (!HasErrors) return false;

        return ErrorsOrEmpty.EnumerateArray().Any(error =>
            error.TryGetProperty("message", out var message) &&
            message.GetString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>Reads a top-level field from the data element.</summary>
    public JsonElement Field(string name) => Data.GetProperty(name);

    /// <summary>True when a top-level data field is present and null.</summary>
    public bool IsFieldNull(string name) =>
        HasData && Body.GetProperty("data").TryGetProperty(name, out var field) && field.ValueKind == JsonValueKind.Null;

    /// <summary>Parses a raw payload into a result.</summary>
    public static GraphQLResult Parse(HttpStatusCode statusCode, string rawBody)
    {
        try
        {
            using var document = JsonDocument.Parse(rawBody);
            return new GraphQLResult(statusCode, rawBody, document.RootElement.Clone());
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Response is not valid JSON (HTTP {(int)statusCode}): {rawBody}", ex);
        }
    }

    /// <summary>Builds a result from an HTTP response message.</summary>
    public static async Task<GraphQLResult> FromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return Parse(response.StatusCode, rawBody);
    }

    /// <inheritdoc />
    public override string ToString() => $"HTTP {(int)StatusCode} {RawBody}";
}

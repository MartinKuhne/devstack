using System.Net;
using System.Text.RegularExpressions;

namespace DevStack.Mcp.Logging;

/// <summary>
/// Provides utility methods for sanitizing user-provided input before logging
/// to prevent log forging/injection attacks (CWE-117) and HTML injection in log viewers.
/// </summary>
public static class LogSanitizer
{
    private static readonly Regex ControlCharRegex = new(@"[\r\n\t\x00-\x1F\x7F-\x9F]", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes user input for plain-text and HTML log viewers by removing line breaks
    /// and control characters, and applying HTML encoding.
    /// </summary>
    /// <param name="input">Untrusted string input from HTTP requests or user payloads.</param>
    /// <returns>A clean, encoded string safe for log templates and semantic properties.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // 1. Remove line breaks and control characters to prevent log forging (CWE-117)
        var lineBreaksRemoved = input
            .Replace(Environment.NewLine, string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);

        var controlCharsCleaned = ControlCharRegex.Replace(lineBreaksRemoved, string.Empty);

        // 2. HTML encode user input to prevent HTML injection in web/HTML log dashboards
        return WebUtility.HtmlEncode(controlCharsCleaned);
    }
}

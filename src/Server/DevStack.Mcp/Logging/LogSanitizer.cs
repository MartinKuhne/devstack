using System.Net;
using System.Text.RegularExpressions;

namespace DevStack.Mcp.Logging;

/// <summary>
/// Provides utility methods for sanitizing untrusted user-provided input before writing to log sinks.
/// 
/// OWASP Vulnerability & Rule:
/// - OWASP: Log Injection / Log Forging
/// - Common Weakness Enumeration: CWE-117 (Improper Output Neutralization for Logs)
/// 
/// OWASP Recommendation & Defensive Strategy:
/// 1. Plain-Text Logs: Line breaks (\r, \n, Environment.NewLine) and control characters must be removed
///    from user input using string replacements to prevent attackers from forging fake log entries or splitting log lines.
/// 2. HTML Log Dashboards: User input must be HTML encoded (using WebUtility.HtmlEncode or similar)
///    before logging to prevent arbitrary HTML or script injection when viewing logs in web-based log viewers.
/// 3. Semantic / Structured Logging: User inputs should be bound as explicit, sanitized semantic parameters
///    within structured log templates (e.g. {RequestMethod}, {RequestPath}) rather than concatenated strings.
/// </summary>
public static class LogSanitizer
{
    // Regex matching non-printable ASCII and control characters (\r, \n, \t, 0x00-0x1F, 0x7F-0x9F)
    private static readonly Regex ControlCharRegex = new(@"[\r\n\t\x00-\x1F\x7F-\x9F]", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes an untrusted string input for plain-text and HTML log viewers according to OWASP CWE-117 recommendations.
    /// </summary>
    /// <param name="input">Untrusted string input derived from HTTP requests, headers, paths, or user payloads.</param>
    /// <returns>A safe, sanitized, and HTML-encoded string suitable for semantic logging templates.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // OWASP Recommendation Step 1: Remove line breaks (\r, \n, Environment.NewLine) to prevent log forging in plain-text logs
        var lineBreaksRemoved = input
            .Replace(Environment.NewLine, string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);

        // Strip non-printable ASCII and control characters
        var controlCharsCleaned = ControlCharRegex.Replace(lineBreaksRemoved, string.Empty);

        // OWASP Recommendation Step 2: HTML encode user input to prevent HTML/XSS injection in web/HTML log dashboards
        return WebUtility.HtmlEncode(controlCharsCleaned);
    }
}

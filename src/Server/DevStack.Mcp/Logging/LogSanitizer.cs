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
/// 2. Semantic / Structured Logging: User inputs should be bound as explicit, sanitized semantic parameters
///    within structured log templates (e.g. {RequestMethod}, {RequestPath}) rather than concatenated strings.
/// </summary>
public static class LogSanitizer
{
    // Regex matching non-printable ASCII and control characters (\r, \n, \t, 0x00-0x1F, 0x7F-0x9F)
    private static readonly Regex ControlCharRegex = new(@"[\r\n\t\x00-\x1F\x7F-\x9F]", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes an untrusted string input for log viewers according to OWASP CWE-117 recommendations.
    /// </summary>
    /// <param name="input">Untrusted string input derived from HTTP requests, headers, paths, or user payloads.</param>
    /// <returns>A safe, sanitized string suitable for semantic logging templates.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // OWASP Recommendation: Remove line breaks (\r, \n, Environment.NewLine) to prevent log forging in plain-text logs
        var lineBreaksRemoved = input
            .Replace(Environment.NewLine, string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);

        // Strip non-printable ASCII and control characters
        return ControlCharRegex.Replace(lineBreaksRemoved, string.Empty);
    }
}

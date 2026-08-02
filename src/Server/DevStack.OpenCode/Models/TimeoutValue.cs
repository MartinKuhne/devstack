namespace DevStack.OpenCode.Models;

/// <summary>
/// Represents a value that may be either a positive integer (milliseconds) or
/// the literal <c>false</c> meaning "disabled". Mirrors the OpenCode
/// <c>timeout</c> / <c>headerTimeout</c> union type.
/// </summary>
[JsonConverter(typeof(TimeoutValueConverter))]
public readonly struct TimeoutValue : IEquatable<TimeoutValue>
{
    private TimeoutValue(bool disabled, int milliseconds)
    {
        Disabled = disabled;
        Milliseconds = milliseconds;
    }

    /// <summary>True when the timeout is explicitly disabled.</summary>
    public bool Disabled { get; }

    /// <summary>Timeout in milliseconds (defined only when <see cref="Disabled"/> is false).</summary>
    public int Milliseconds { get; }

    /// <summary>True when a positive millisecond value is set.</summary>
    public bool HasValue => !Disabled && Milliseconds > 0;

    /// <summary>Builds a timeout representing the given millisecond value.</summary>
    public static TimeoutValue FromMilliseconds(int ms)
    {
        if (ms <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ms), ms, "Timeout must be a positive integer.");
        }

        return new TimeoutValue(false, ms);
    }

    /// <summary>Builds a disabled timeout (serializes as JSON <c>false</c>).</summary>
    public static TimeoutValue Disable() => new(true, 0);

    public bool Equals(TimeoutValue other) => Disabled == other.Disabled && Milliseconds == other.Milliseconds;
    public override bool Equals(object? obj) => obj is TimeoutValue v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(Disabled, Milliseconds);
    public override string ToString() => Disabled ? "disabled" : $"{Milliseconds}ms";
}

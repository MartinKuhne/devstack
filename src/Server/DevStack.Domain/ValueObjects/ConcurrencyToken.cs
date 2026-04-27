namespace DevStack.Domain.ValueObjects;

public readonly struct ConcurrencyToken
{
    private readonly byte[] _value;

    public ConcurrencyToken(byte[] value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override bool Equals(object? obj)
    {
        return obj is ConcurrencyToken other &&
               _value.SequenceEqual(other._value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(ConcurrencyToken left, ConcurrencyToken right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ConcurrencyToken left, ConcurrencyToken right)
    {
        return !(left == right);
    }
}

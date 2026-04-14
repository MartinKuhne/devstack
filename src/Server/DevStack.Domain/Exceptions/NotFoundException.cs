namespace DevStack.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string type, object key)
        : base($"Entity '{type}' with key '{key}' was not found.")
    {
        Type = type;
        Key = key;
    }

    public string Type { get; }
    public object Key { get; }
}

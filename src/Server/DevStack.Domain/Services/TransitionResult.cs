#nullable disable

namespace DevStack.Domain.Services;

public readonly struct TransitionResult<T>
{
    private TransitionResult(T value, IReadOnlyList<string> errors)
    {
        Value = value;
        Errors = errors;
        IsSuccess = errors.Count == 0;
    }

    public T Value { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool IsSuccess { get; }

    public static TransitionResult<T> Success(T value) => new(value, Array.Empty<string>());
    public static TransitionResult<T> Failure(IReadOnlyList<string> errors) => new(default!, errors);
}

public readonly struct Unit
{
    public static readonly Unit Value = new();
}

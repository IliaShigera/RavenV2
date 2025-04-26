namespace Raven.Core.Modes;

public readonly struct Result<T>
{
    private Result(bool ok, T data, string? error)
    {
        Ok = ok;
        Data = data;
        Error = error;
    }

    public bool Ok { get; }
    public T Data { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<Empty> Success() => new(true, default, null);
    public static Result<T> Failure(string error) => new(false, default!, error);
}

public readonly struct Empty{}
namespace HidroBairro.Application.Common;

public class ServiceResult
{
    public bool Succeeded { get; protected init; }
    public string? Error { get; protected init; }
    public bool NotFound { get; protected init; }
    public bool Conflict { get; protected init; }
    public bool Forbidden { get; protected init; }

    public static ServiceResult Success() => new() { Succeeded = true };

    public static ServiceResult Failure(string error) => new()
    {
        Succeeded = false,
        Error = error
    };

    public static ServiceResult AsNotFound(string error) => new()
    {
        Succeeded = false,
        NotFound = true,
        Error = error
    };

    public static ServiceResult AsConflict(string error) => new()
    {
        Succeeded = false,
        Conflict = true,
        Error = error
    };

    public static ServiceResult AsForbidden(string error) => new()
    {
        Succeeded = false,
        Forbidden = true,
        Error = error
    };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        Succeeded = true,
        Data = data
    };

    public new static ServiceResult<T> Failure(string error) => new()
    {
        Succeeded = false,
        Error = error
    };

    public new static ServiceResult<T> AsNotFound(string error) => new()
    {
        Succeeded = false,
        NotFound = true,
        Error = error
    };

    public new static ServiceResult<T> AsConflict(string error) => new()
    {
        Succeeded = false,
        Conflict = true,
        Error = error
    };

    public new static ServiceResult<T> AsForbidden(string error) => new()
    {
        Succeeded = false,
        Forbidden = true,
        Error = error
    };
}

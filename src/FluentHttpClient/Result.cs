
//https://github.com/joseftw/jos.result
using System.Net;

namespace FluentHttpClient;

public abstract class Result
{
    public bool Success { get; protected set; }
    public bool Retryable { get; protected set; }
    public bool Failure => !Success;
}

public abstract class Result<T> : Result
{
    private T _data;

    protected Result(T data)
    {
        Data = data;
    }

    public T Data
    {
        get => Success ? _data : throw new Exception($"You can't access .{nameof(Data)} when .{nameof(Success)} is false");
        set => _data = value;
    }
}

public class SuccessResult : Result
{
    public SuccessResult()
    {
        Success = true;
    }
}

public class SuccessResult<T> : Result<T>
{
    public SuccessResult(T data) : base(data)
    {
        Success = true;
    }

    public static implicit operator SuccessResult(SuccessResult<T> successResult)
    {
        return new SuccessResult();
    }
}

public class ErrorResult : Result, IErrorResult
{
    public ErrorResult(string message) : this(message, false)
    {

    }

    public ErrorResult(string message, bool retryable) : this(message, retryable, Array.Empty<Error>())
    {

    }

    public ErrorResult(string message, bool retryable, IReadOnlyCollection<Error> errors)
    {
        Message = message;
        Success = false;
        Retryable = retryable;
        Errors = errors ?? Array.Empty<Error>();
    }

    public string Message { get; }
    public IReadOnlyCollection<Error> Errors { get; }

    public virtual ErrorResult<T> ToGeneric<T>()
    {
        return new ErrorResult<T>(Message, Retryable, Errors);
    }
}

public class ErrorResult<T> : Result<T>, IErrorResult
{
    public ErrorResult(string message) : this(message, false, Array.Empty<Error>())
    {

    }

    public ErrorResult(string message, bool retryable) : this(message, retryable, Array.Empty<Error>())
    {

    }

    public ErrorResult(string message, bool retryable, IReadOnlyCollection<Error> errors) : base(default!)
    {
        Message = message;
        Success = false;
        Retryable = retryable;
        Errors = errors ?? Array.Empty<Error>();
    }

    public string Message { get; set; }
    public IReadOnlyCollection<Error> Errors { get; }

    public static implicit operator ErrorResult(ErrorResult<T> errorResult)
    {
        return new ErrorResult(errorResult.Message, errorResult.Retryable, errorResult.Errors);
    }

    public virtual ErrorResult<TType> ToGeneric<TType>()
    {
        return new ErrorResult<TType>(Message, Retryable, Errors);
    }
}

public class NotFoundResult<T> : ErrorResult<T>
{
    public NotFoundResult(string message) : base(message)
    {
    }

    public NotFoundResult(string message, IReadOnlyCollection<Error> errors) : base(message, false, errors)
    {
    }
}

public class HttpErrorResult : ErrorResult
{
    public HttpStatusCode StatusCode { get; }

    public HttpErrorResult(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpErrorResult(string message, bool retryable, IReadOnlyCollection<Error> errors, HttpStatusCode statusCode) : base(message, retryable, errors)
    {
        StatusCode = statusCode;
    }
}

public class Error
{
    public Error(string details) : this(null!, details)
    {

    }

    public Error(string code, string details)
    {
        Code = code;
        Details = details;
    }

    public string Code { get; }
    public string Details { get; }
}

internal interface IErrorResult
{
    string Message { get; }
    IReadOnlyCollection<Error> Errors { get; }
}

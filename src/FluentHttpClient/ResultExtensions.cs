
using System.Net;
using Wrapture;

namespace FluentHttpClient;

public class SuccessResult : Result
{
    public SuccessResult(): base(true, null!)
    {
       
    }
}

public class SuccessResult<T> : Result<T>
{
    public SuccessResult(T data) : base(true, data, null!)
    {
       
    }

    public static implicit operator SuccessResult(SuccessResult<T> successResult)
    {
        return new SuccessResult();
    }
}

public class ErrorResult : Result, IErrorResult
{
    public ErrorResult(string error) : this(error, false)
    {

    }

    public ErrorResult(string error, bool retryable) : this(error, retryable, Array.Empty<Error>())
    {

    }

    public ErrorResult(string error, bool retryable, IReadOnlyCollection<Error> errors)
        : base(false, null!)
    {
        Error = error;
        Retryable = retryable;
        Errors = errors ?? Array.Empty<Error>();
    }

    public bool Retryable { get; private set; }
    public IReadOnlyCollection<Error> Errors { get; }

    public virtual ErrorResult<T> ToGeneric<T>()
    {
        return new ErrorResult<T>(Error, Retryable, Errors);
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

    public ErrorResult(string error, bool retryable, IReadOnlyCollection<Error> errors) 
        : base(false, default!, error)
    {
        Error = error;
        Retryable = retryable;
        Errors = errors ?? Array.Empty<Error>();
    }

    public bool Retryable { get; private set; }
    public IReadOnlyCollection<Error> Errors { get; }

    public static implicit operator ErrorResult(ErrorResult<T> errorResult)
    {
        return new ErrorResult(errorResult.Error, errorResult.Retryable, errorResult.Errors);
    }

    public virtual ErrorResult<TType> ToGeneric<TType>()
    {
        return new ErrorResult<TType>(Error, Retryable, Errors);
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
    string Error { get; }
    IReadOnlyCollection<Error> Errors { get; }
}

using System.Net;

namespace FluentHttpClient;

public class HttpCallFailure
{
    /// <summary>
    /// The HTTP status code returned by the endpoint.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Indicates whether the failure is retryable based on the status code.
    /// </summary>
    public bool IsRetryable { get; }

    /// <summary>
    /// The error message describing the issue.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// The original HttpResponseMessage returned by the endpoint.
    /// </summary>
    public HttpResponseMessage? OriginalResponse { get; }

    /// <summary>
    /// The URL of the endpoint that was called.
    /// </summary>
    public Uri? Endpoint { get; }

    /// <summary>
    /// The timestamp when the failure occurred.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpCallFailure"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="endpoint">The URL of the endpoint.</param>
    /// <param name="originalResponse">The original HttpResponseMessage.</param>
    public HttpCallFailure(HttpStatusCode statusCode, string errorMessage, Uri? endpoint = null, HttpResponseMessage? originalResponse = null)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        OriginalResponse = originalResponse;
        IsRetryable = statusCode.IsRetryable();
    }

    /// <summary>
    /// Returns a developer-friendly string representation of the failure.
    /// </summary>
    /// <returns>A string describing the failure.</returns>
    public override string ToString()
    {
        return $"""
        HTTP Call Failure:
        Endpoint: {Endpoint}
        Status Code: {(int)StatusCode} ({StatusCode})
        Is Retryable: {IsRetryable}
        Error Message: {ErrorMessage}
        Timestamp: {Timestamp:O}
        """;
    }
}
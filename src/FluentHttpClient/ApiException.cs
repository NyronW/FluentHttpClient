using System.Net;

namespace FluentHttpClient;

public class ApiException : Exception
{
    public HttpStatusCode Status { get; }

    public HttpResponseMessage ResponseMessage { get; }


    /// <summary>Construct an instance.</summary>
    /// <param name="responseMessage">The HTTP HttpResponseMessage which caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception (or <c>null</c> for no inner exception).</param>
    public ApiException(HttpResponseMessage responseMessage, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ResponseMessage = responseMessage;
        Status = responseMessage.StatusCode;
    }
}

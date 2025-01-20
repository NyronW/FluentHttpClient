using System.Net.Http.Formatting;

namespace FluentHttpClient;

public interface IFluentHttpClient
{
    FilterCollection Filters { get; }

    MediaTypeFormatterCollection Formatters { get; }

    IAssignEndpoint Endpoint(string endpoint);
    IAssignEndpoint UsingBaseUrl();

    Uri? BaseUrl { get; }
    Uri? RequestUrl { get; }

    bool HasHeader(string name);
}

public interface IFluentHttpClient<TConsumer> : IFluentHttpClient
{
    
}


public interface ISetHttpHandler : IAssignEndpoint
{
    ISendOrCancel SetHandler();
}

public interface IAssignEndpoint : IAssignHeaders, IAssignArguments, 
    ISetCorrelationId, ISetContentType, ISendRequest, 
    ISendRequestWithBody, ISendAuthenticateOrAttached, IAttachFiles
{
    ISendOrCancel SetUserAgent(string userAgent);
    Uri GetRequestUrl();
}

public interface IAssignHeaders
{
    IAssignEndpoint WithHeader(string key, string value);
    IAssignEndpoint WithHeaders(IEnumerable<KeyValuePair<string, string>>? headers);
}

public interface IAssignArguments
{
    IAssignEndpoint WithArgument<TArgument>(string key, TArgument value);
    IAssignEndpoint WithArguments(IEnumerable<KeyValuePair<string, object?>> arguments);
    IAssignEndpoint WithArguments(object? arguments);
}

public interface ISetCorrelationId
{
    IAssignEndpoint WithCorrelationId(string correlationId);
    IAssignEndpoint WithGeneratedCorelationId();
}

public interface ISetContentType
{
    ISendOrCancel UsingContentType(string contentType);
    ISendOrCancel UsingXmlFormat();
    ISendOrCancel UsingJsonFormat();
}


public interface ISendOrCancel : IAssignCancellationToken, ISendOrAuthenticate
{

}

public interface IAssignCancellationToken
{
    ISendOrAuthenticate WithCancellationToken(CancellationToken token);
}

public interface ISendOrAuthenticate : ISendRequest, ISendRequestWithBody, ISetAuhtentication, IAttachFiles
{

}

public interface ISendAuthenticateOrAttached : ISendRequest, ISendRequestWithBody, ISetAuhtentication
{

}

public interface IAttachFiles
{
    ISendFileActions AttachFiles(string[] files);
    ISendFileActions AttachFile(string fullPath);
    ISendFileActions AttachFile(string name, Stream stream);
}


public interface ISendFileActions : IAssignCancellationToken, ISendRequestWithBody, ISetAuhtentication, IUploadFile
{

}

public interface ISendRequest
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead);
    Task<HttpResponseMessage> GetAsync(HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead);
    Task<TResponse> GetAsync<TResponse>();
    Task<Stream> GetStreamAsync(HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead);
}

public interface ISendRequestWithBody
{
    Task<TResponse> PatchAsync<TRequest, TResponse>(TRequest request);
    Task<HttpResponseMessage> PatchAsync<TRequest>(TRequest request);
    Task<TResponse> PutAsync<TRequest, TResponse>(TRequest request);
    Task<HttpResponseMessage> PutAsync<TRequest>(TRequest request);
    Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request);
    Task<HttpResponseMessage> PostAsync<TRequest>(TRequest request);
    Task<HttpResponseMessage> PostAsync(HttpContent content);
    Task<TResponse> PostAsync<TResponse>(HttpContent content);
}

public interface IUploadFile
{
    Task<HttpResponseMessage> PostAsync();
}


public interface ISendActions : ISendRequest, ISendRequestWithBody, IAttachFiles
{

}

public interface ISetAuhtentication
{
    ISendActions UsingBasicAuthentication(string userName, string password);
    ISendActions UsingBearerToken(string token);
    ISendActions UsingIdentityServer(string url, string clientId, string clientSecret, params string[] scopes);
}



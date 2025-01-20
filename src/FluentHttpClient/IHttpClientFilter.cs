namespace FluentHttpClient;

public interface IHttpClientFilter
{
    [Obsolete("This method is deprecated and will be removed in the next version. Use OnRequest(FluentHttpRequest request) instead.")]
    void OnRequest(HttpRequestMessage request)
    {

    }

    void OnRequest(FluentHttpRequest request)
    {

    }

    void OnBeforeRequest(FluentHttpModel model)
    {

    }

    [Obsolete("This method is deprecated and will be removed in the next version. Use OnRequest(FluentHttpResponse response) instead.")]
    void OnResponse(HttpResponseMessage response)
    {

    }
    void OnResponse(FluentHttpResponse response)
    {

    }
}


public class FluentHttpModel
{
    public IDictionary<string, object?> Properties { get; }
    public IServiceProvider ServiceProvider { get; }
    public string Identifier { get; }
    public HttpClient Client { get; }

    public FluentHttpModel(
        string identifier,
        HttpClient client,
        IDictionary<string, object?> properties,
        IServiceProvider serviceProvider)
    {
        Identifier = identifier;
        Client = client;
        Properties = properties;
        ServiceProvider = serviceProvider;
    }
}

public sealed class FluentHttpRequest(
    string identifier,
    HttpClient client,
    HttpRequestMessage requestMessage,
    IDictionary<string, object?> properties,
    IServiceProvider serviceProvider) : FluentHttpModel(identifier, client,properties, serviceProvider)
{
    public HttpRequestMessage RequestMessage { get; } = requestMessage;
}

public sealed class FluentHttpResponse(FluentHttpRequest request, HttpResponseMessage responseMessage)
{
    public FluentHttpRequest Request { get; } = request;
    public HttpResponseMessage ResponseMessage { get; } = responseMessage;
}

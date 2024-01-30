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

    void OnResponse(HttpResponseMessage response)
    {

    }
}


public class FluentHttpModel
{
    public IDictionary<string, object?> Properties { get; }
    public string Identifier { get; }
    public HttpClient Client { get; }

    public FluentHttpModel(string identifier, HttpClient client,IDictionary<string, object?> properties)
    {
        Identifier = identifier;
        Client = client;
        Properties = properties;
    }
}

public class FluentHttpRequest : FluentHttpModel
{
    public HttpRequestMessage RequestMessage { get; }

    public FluentHttpRequest(string identifier, HttpClient client, HttpRequestMessage requestMessage,
        IDictionary<string, object?> properties) :base(identifier, client,properties)
    {
        RequestMessage = requestMessage;
    }
}
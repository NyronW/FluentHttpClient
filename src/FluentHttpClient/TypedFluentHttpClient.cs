using System.Net.Http.Formatting;

namespace FluentHttpClient;

internal sealed class TypedFluentHttpClient<TConsumer> : IFluentHttpClient<TConsumer>
{
    private readonly IFluentHttpClient _innerClient;

    internal TypedFluentHttpClient(IFluentHttpClient innerClient) => _innerClient = innerClient;

    public FilterCollection Filters => _innerClient.Filters;

    public MediaTypeFormatterCollection Formatters => _innerClient.Formatters;

    public IAssignEndpoint Endpoint(string endpoint) => _innerClient.Endpoint(endpoint);
    public Uri GetRequestUrl() => _innerClient.RequestUrl!;

    Uri? IFluentHttpClient.BaseUrl => _innerClient.BaseUrl;

    public Uri? RequestUrl => _innerClient.RequestUrl;

    public bool HasHeader(string name) => _innerClient.HasHeader(name);

    public IAssignEndpoint UsingBaseUrl() => _innerClient.UsingBaseUrl();
}



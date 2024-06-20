using Microsoft.Extensions.DependencyInjection;

namespace FluentHttpClient;

internal sealed class HttpClientDescriptor
{
    public string BaseUrl { get; set; } = null!;
    public Dictionary<string, object> Headers { get; set; } = [];
    public FilterCollection Filters { get; set; } = [];
    public TimeSpan? Timeout { get; set; } = null!;
}

public sealed class FluentHttpClientFactory : IFluentHttpClientFactory,
    IFluentHttpClientBuilder,
    IFluentClientBuilderAction,
    IHandlerRegistration,
    ISetDefaultHeader,
    IRegisterHttpClient
{
    internal static IDictionary<string, HttpClientDescriptor> ClientDescriptions = new Dictionary<string, HttpClientDescriptor>();
    internal static IDictionary<string, IDictionary<string, object?>> _properties = new Dictionary<string, IDictionary<string, object?>>();
    internal static FilterCollection DefaultFilters { get; } = [];


    internal Func<HttpMessageHandler> PrimaryMessageHandler { get; private set; } = null!;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, object> _headers = [];
    private string _identifier = null!;
    private string _url = null!;
    private TimeSpan _timeout;
    private FilterCollection _filters = [];

    public FluentHttpClientFactory(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
    }

    internal static void AddFilter(Type filterType)
    {
        if (filterType == null) return;
        if (!typeof(IHttpClientFilter).IsAssignableFrom(filterType)) return;
        if (DefaultFilters.Contains(filterType)) return;

        DefaultFilters.Add(filterType);
    }

    #region IFluentHttpClientFactory 
    public IFluentHttpClient Get<TType>() where TType : class => Get(typeof(TType).FullName!);

    public IFluentHttpClient Get(Type type) => Get(type.FullName!);

    public IFluentHttpClient Get(string identifier, bool createIfNotFound = true)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException($"'{nameof(identifier)}' cannot be null or whitespace.", nameof(identifier));
        }

        if (!ClientDescriptions.ContainsKey(identifier) && !createIfNotFound)
        {
            throw new ArgumentException($"No client configuration found for identifier:'{identifier}'.", nameof(identifier));
        }

        var descriptor = ClientDescriptions.TryGetValue(identifier, out HttpClientDescriptor? value) ? value : null;

        var http = _httpClientFactory.CreateClient(identifier);
        http.DefaultRequestHeaders.Clear();

        var props = _properties.TryGetValue(identifier, out var dict) ? dict : new Dictionary<string, object?>();
        var client = new FluentHttpClient(identifier, http, props, _serviceProvider);

        if (descriptor != null)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.BaseUrl)) http.BaseAddress = new Uri(descriptor.BaseUrl);
            if (descriptor.Timeout != TimeSpan.Zero) http.Timeout = descriptor.Timeout!.Value;

            foreach (var header in descriptor.Headers)
                http.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToString());

            if (descriptor.Filters.Any())
                foreach (var filter in descriptor.Filters) client.Filters.Add(filter);
        }

        foreach (var filter in DefaultFilters)
            client.Filters.Add(filter);

        return client;
    }
    #endregion

    #region IFluentHttpClientBuilder
    public IFluentClientBuilderAction CreateClient<TType>() where TType : class => CreateClient(typeof(TType).FullName!);

    public IFluentClientBuilderAction CreateClient(Type type) => CreateClient(type.FullName!);

    public IFluentClientBuilderAction CreateClient(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException($"'{nameof(identifier)}' cannot be null or whitespace.", nameof(identifier));
        }

        _identifier = identifier;
        _headers = [];
        _url = string.Empty;
        _timeout = TimeSpan.Zero;
        _filters = [];

        return this;
    }

    public ISetDefaultHeader WithBaseUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
        }

        _url = url;

        return this;
    }

    public ISetDefaultHeader WithHeader(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
        }

        if (!_headers.ContainsKey(key))
            _headers.Add(key, value);

        return this;
    }

    public ISetDefaultHeader AddFilter<TFilter>() where TFilter : IHttpClientFilter
    {
        if (!_filters.Contains(typeof(TFilter)) && !DefaultFilters.Contains(typeof(TFilter)))
        {
            _filters.Add(typeof(TFilter));
        }

        return this;
    }

    public IHandlerRegistration WithTimeout(int timeout)
    {
        if (timeout < 0)
        {
            throw new ArgumentException($"'{nameof(timeout)}' cannot be less than zero.", nameof(timeout));
        }

        _timeout = TimeSpan.FromSeconds(timeout);

        return this;
    }

    public IHandlerRegistration WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public IRegisterHttpClient WithHandler(Func<HttpMessageHandler> configureHandler)
    {
        PrimaryMessageHandler = configureHandler;
        return this;
    }

    public IRegisterHttpClient WithHandler<THandler>() where THandler : HttpMessageHandler
    {
        PrimaryMessageHandler = () => _serviceProvider.GetService<THandler>()!;
        return this;
    }

    public ISetDefaultHeader WithProperty<TValue>(string name, TValue value)
    {
        var prop = _properties.TryGetValue(_identifier, out var dict) ? dict : new Dictionary<string, object?>();
        prop.Add(name, value);

        _properties[_identifier] = prop;
        return this;
    }

    public ISetDefaultHeader WithProperties(IEnumerable<KeyValuePair<string, object?>> properties)
    {
        if (properties == null) return this;

        var prop = _properties.TryGetValue(_identifier, out var dict) ? dict : new Dictionary<string, object?>();
        prop.AddRange(properties.ToArray());

        _properties.Add(_identifier, prop);
        return this;
    }

    public void Register()
    {
        if (IsRegistered) return;

        ClientDescriptions.Add(_identifier, new HttpClientDescriptor
        {
            Timeout = _timeout,
            BaseUrl = _url,
            Headers = _headers,
            Filters = _filters
        });
    }
    #endregion

    internal bool IsRegistered => ClientDescriptions.ContainsKey(_identifier);
    internal bool HasBaseUrl => !string.IsNullOrWhiteSpace(_url);
}
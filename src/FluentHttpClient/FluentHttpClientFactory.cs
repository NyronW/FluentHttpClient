namespace FluentHttpClient;

internal sealed class HttpClientDescriptor
{
    public string BaseUrl { get; set; } = null!;
    public Dictionary<string, object> Headers { get; set; } = new();
    public TimeSpan? Timeout { get; set; } = null!;
}

public sealed class FluentHttpClientFactory : IFluentHttpClientFactory,
    IFluentHttpClientBuilder,
    IFluentClientBuilderAction,
    ISetDefaultHeader,
    IRegisterHttpClient
{
    internal static IDictionary<string, HttpClientDescriptor> ClientDescriptions = new Dictionary<string, HttpClientDescriptor>();
    internal static FilterCollection DefaultFilters { get; } = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, object> _headers = new();
    private string _identifier = null!;
    private string _url = null!;
    private TimeSpan _timeout;


    public FluentHttpClientFactory(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
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

        var descriptor = ClientDescriptions[identifier];
        if (descriptor == null && !createIfNotFound)
        {
            throw new ArgumentException($"No client configuration found for identifier:'{nameof(identifier)}'.", nameof(identifier));
        }

        var http = _httpClientFactory.CreateClient(identifier);
        http.DefaultRequestHeaders.Clear();

        if (descriptor != null)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.BaseUrl)) http.BaseAddress = new Uri(descriptor.BaseUrl);
            if (descriptor.Timeout != null) http.Timeout = descriptor.Timeout.Value;

            foreach (var header in descriptor.Headers)
                http.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToString());
        }

        var client = new FluentHttpClient(http, _serviceProvider);

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
        _headers = new Dictionary<string, object>();
        _url = string.Empty;
        _timeout = TimeSpan.Zero;

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

    public ISetDefaultHeader AddFilter<TFilter>()
    {
        if (!DefaultFilters.Contains(typeof(TFilter)))
            DefaultFilters.Add(typeof(TFilter));

        return this;
    }

    public IRegisterHttpClient WithTimeout(int timeout)
    {
        if (timeout < 0)
        {
            throw new ArgumentException($"'{nameof(timeout)}' cannot be less than zero.", nameof(timeout));
        }

        _timeout = TimeSpan.FromSeconds(timeout);

        return this;
    }

    public IRegisterHttpClient WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public void Register()
    {
        if (IsRegistered) return;

        ClientDescriptions.Add(_identifier, new HttpClientDescriptor
        {
            Timeout = _timeout,
            BaseUrl = _url,
            Headers = _headers
        });
    }
    #endregion

    internal bool IsRegistered => ClientDescriptions.ContainsKey(_identifier);
    internal bool HasBaseUrl => !string.IsNullOrWhiteSpace(_url);
}
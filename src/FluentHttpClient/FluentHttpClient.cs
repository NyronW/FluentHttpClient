﻿using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Net.Http.Formatting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace FluentHttpClient;

public class FluentHttpClient : IFluentHttpClient,
    IAssignEndpoint,
    ISendOrCancel,
    ISendActions,
    ISendFileActions
{
    #region Fields & Properties
    private readonly Version version = typeof(FluentHttpClient).GetTypeInfo().Assembly.GetName().Version!;
    private readonly string _identifier;
    private readonly HttpClient _client;
    private readonly IDictionary<string, object?> _factoryProperties;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, object?> _arguments = [];
    private readonly Dictionary<string, string> _headers = [];
    private CancellationToken _token;
    private string _contentType = "application/json";
    private string _endpoint = null!;
    private ICollection<IHttpClientFilter> _filters = null!;
    private string _identityUrl = null!;
    private string _clientId = null!;
    private string _clientSecret = null!;
    private string[] _scopes = null!;
    private bool _requestToken;
    private readonly List<KeyValuePair<string, Stream>> _files = [];
    #endregion



    public FluentHttpClient(string identifier, HttpClient client, IDictionary<string, object?> factoryProperties, IServiceProvider serviceProvider)
    {
        _identifier = identifier ?? Guid.NewGuid().ToString();
        _client = client;
        _factoryProperties = factoryProperties;
        _serviceProvider = serviceProvider;

        SetDefaultUserAgent();
    }

    public FilterCollection Filters { get; } = [];

    public Collection<MediaTypeFormatter> Formatters { get; } = [
        new JsonMediaTypeFormatter { SerializerSettings = HttpExtensions.JsonSerializerSettings },
        new XmlMediaTypeFormatter(),
        new FormUrlEncodedMediaTypeFormatter()
    ];

    public ILogger GetLogger() => _serviceProvider.GetRequiredService<ILogger<FluentHttpClient>>();

    public Uri? BaseUrl => _client.BaseAddress;
    public Uri? RequestUrl => BuildUrl(BaseUrl, _endpoint);

    public bool HasHeader(string name)
    {
        return _headers.ContainsKey(name);
    }

    public IAssignEndpoint Endpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException($"'{nameof(endpoint)}' cannot be null or whitespace.", nameof(endpoint));
        }

        _endpoint = endpoint;
        _requestToken = false;
        _arguments.Clear();
        _contentType = "application/json";

        foreach (var header in _headers)
            _client.DefaultRequestHeaders.Remove(header.Key);

        _headers.Clear();
        _files.Clear();
        _scopes = null!;

        return this;
    }


    public Uri GetRequestUrl() => BuildUrl(BaseUrl, _endpoint);

    public IAssignEndpoint UsingBaseUrl()
    {
        _requestToken = false;
        _arguments.Clear();
        _contentType = "application/json";

        foreach (var header in _headers)
            _client.DefaultRequestHeaders.Remove(header.Key);

        _headers.Clear();
        _files.Clear();
        _scopes = null!;

        return this;
    }

    public ISendOrCancel SetUserAgent(string userAgent)
    {
        _client.DefaultRequestHeaders.Remove("User-Agent");
        _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        return this;
    }

    #region ISetAuhtentication
    public ISendActions UsingBasicAuthentication(string userName, string password)
    {
        var byteArray = Encoding.ASCII.GetBytes($"{userName}:{password}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        _requestToken = false;

        return this;
    }

    public ISendActions UsingBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _requestToken = false;

        return this;
    }

    public ISendActions UsingIdentityServer(string url, string clientId, string clientSecret, params string[] scopes)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException($"'{nameof(clientId)}' cannot be null or whitespace.", nameof(clientId));
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new ArgumentException($"'{nameof(clientSecret)}' cannot be null or empty.", nameof(clientSecret));
        }

        if (scopes is null)
        {
            _scopes = Array.Empty<string>();
        }

        _identityUrl = url;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _scopes = scopes;
        _requestToken = true;

        return this;
    }
    #endregion

    #region IAssignHeaders
    public IAssignEndpoint WithHeaders(IEnumerable<KeyValuePair<string, string>>? headers)
    {
        if (headers == null)
        {
            throw new ArgumentException($"'{nameof(headers)}' cannot be null or whitespace.", nameof(headers));
        }

        foreach (var arg in headers)
        {
            if (string.IsNullOrWhiteSpace(arg.Key))
                continue;

            _client.DefaultRequestHeaders.TryAddWithoutValidation(arg.Key, arg.Value);

            _headers[arg.Key] = arg.Value;
        }

        return this;
    }

    public IAssignEndpoint WithHeader(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
        }

        return WithHeaders(new[] { new KeyValuePair<string, string>(key, value) }!);
    }
    #endregion

    #region IAssignArguments
    public IAssignEndpoint WithArgument<TArgument>(string key, TArgument value)
    {
        _arguments.Add(key, value);
        return this;
    }

    public IAssignEndpoint WithArguments(IEnumerable<KeyValuePair<string, object?>>? arguments)
    {
        if (arguments != null)
            _arguments.AddRange(arguments.ToArray()!);

        return this;
    }

    public IAssignEndpoint WithArguments(object? arguments)
    {
        if (arguments == null)
            return this;

        KeyValuePair<string, object?>[] args = arguments.GetKeyValueArguments().ToArray();

        return WithArguments(args);
    }
    #endregion

    #region IAssignCancellationToken
    public ISendOrAuthenticate WithCancellationToken(CancellationToken token)
    {
        _token = token;
        return this;
    }
    #endregion

    #region ISetContentType
    public ISendOrCancel UsingJsonFormat() => UsingContentType("application/json");

    public ISendOrCancel UsingXmlFormat() => UsingContentType("application/xml");

    public ISendOrCancel UsingContentType(string contentType)
    {
        _contentType = contentType;
        return this;
    }
    #endregion

    #region IAttachFiles
    public ISendFileActions AttachFiles(string[] files)
    {
        var streams = files.Select(f => new FileInfo(f)).Select(file => file.Exists
                ? new KeyValuePair<string, Stream>(file.Name, file.OpenRead())
                : throw new FileNotFoundException($"There's no file matching path '{file.FullName}'.")
        );

        _files.AddRange(streams);

        return this;
    }

    public ISendFileActions AttachFile(string fullPath) => AttachFiles(new[] { fullPath });

    public ISendFileActions AttachFile(string name, Stream stream)
    {
        _files.Add(new(name, stream));

        return this;
    }
    #endregion

    #region ISendRequest
    public async Task<HttpResponseMessage> GetAsync(HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        ICollection<IHttpClientFilter> filters = GetFilterInstances();
        using HttpRequestMessage request = BuildRequest(_endpoint, HttpMethod.Get, filters);
        return await SendAsync(request, filters, httpCompletionOption);
    }

    public async Task<TResponse> GetAsync<TResponse>()
    {
        using HttpResponseMessage response = await GetAsync();
        TResponse? model = await response.Content.ReadAsAsync<TResponse>(Formatters, _token);
        return model;
    }

    public async Task<Stream> GetStreamAsync(HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        Uri url = BuildUrl(_client.BaseAddress!, _endpoint).WithArguments(_arguments.ToArray()!);
        return await _client.GetStreamAsync(url);
    }

    public async Task<HttpResponseMessage> DeleteAsync(HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        ICollection<IHttpClientFilter> filters = GetFilterInstances();
        using HttpRequestMessage request = BuildRequest(_endpoint, HttpMethod.Delete, filters);
        return await SendAsync(request, filters, httpCompletionOption);
    }

    private async Task<HttpResponseMessage> SendAsync<TRequest>(string endpoint, HttpMethod method, TRequest? payload)
    {
        ICollection<IHttpClientFilter> filters = GetFilterInstances();
        using HttpRequestMessage request = BuildRequest(endpoint, method, payload, filters);
        return await SendAsync(request, filters);
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) => await SendAsync(request, GetFilterInstances(), httpCompletionOption, cancellationToken);

    internal async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, ICollection<IHttpClientFilter> filters,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken? cancellationToken = default)
    {
        ILogger<FluentHttpClient>? logger = _serviceProvider.GetService<ILogger<FluentHttpClient>>();
        KeyValuePair<string, string> pair = _headers.SingleOrDefault(h => h.Key.Equals(Headers.CorrelationId, StringComparison.OrdinalIgnoreCase));

        using IDisposable disposable = logger!.AddContext(nameof(Headers.CorrelationId), pair.Value ?? Guid.NewGuid().ToString());

        if (request.RequestUri == null)
            request.RequestUri = BuildUrl(_client.BaseAddress!, _endpoint).WithArguments(_arguments.ToArray()!);

        var req = new FluentHttpRequest(_identifier, _client, request, _factoryProperties, _serviceProvider);
        foreach (IHttpClientFilter filter in filters)
        {
            filter.OnRequest(req);
            filter.OnRequest(request);
        }

        request.Headers.TryAddWithoutValidation("content-type", _contentType);

        if (_requestToken)
        {
            AccessToken accessToken = await GetAccessToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        }

        if (_files is { Count: > 0 })
        {
            var content = new MultipartFormDataContent();
            if (request.Content != null)
            {
                content.Add(request.Content);
            }

            foreach (KeyValuePair<string, Stream> file in _files)
            {
                content.Add(new StreamContent(file.Value), file.Key, file.Key);
            }
            request.Content = content;
        }

        logger?.LogDebug("Reqest Header:-");
        foreach (var header in request.Headers)
        {
            logger?.LogDebug("\t{Header}={HeaderValue}", header.Key, header.Value);
        }
        foreach (var header in _client.DefaultRequestHeaders)
        {
            logger?.LogDebug("\t{Header}={HeaderValue}", header.Key, header.Value);
        }

        HttpResponseMessage response = await _client.SendAsync(request, httpCompletionOption, cancellationToken ?? _token);

        var res = new FluentHttpResponse(req, response);
        foreach (IHttpClientFilter filter in _filters)
        {
            filter.OnResponse(res);
            filter.OnResponse(response);
        }

        return response;
    }
    #endregion

    #region ISendRequestWithBody
    public async Task<TResponse> PatchAsync<TRequest, TResponse>(TRequest request)
    {
        using HttpResponseMessage response = await PatchAsync(request);
        TResponse? model = await response.Content.ReadAsAsync<TResponse>(Formatters, _token);
        return model;
    }

    public async Task<HttpResponseMessage> PatchAsync<TRequest>(TRequest request)
        => await SendAsync(_endpoint, HttpMethod.Patch, request);

    public async Task<TResponse> PutAsync<TRequest, TResponse>(TRequest request)
    {
        using HttpResponseMessage response = await PutAsync(request);
        TResponse? model = await response.Content.ReadAsAsync<TResponse>(Formatters, _token);
        return model;
    }

    public async Task<HttpResponseMessage> PutAsync<TRequest>(TRequest request)
     => await SendAsync(_endpoint, HttpMethod.Put, request);

    public async Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request)
    {
        using HttpResponseMessage response = await PostAsync(request);
        TResponse? model = await response.Content.ReadAsAsync<TResponse>(Formatters, _token);
        return model;
    }

    public async Task<HttpResponseMessage> PostAsync<TRequest>(TRequest request)
        => await SendAsync(_endpoint, HttpMethod.Post, request);

    public async Task<HttpResponseMessage> PostAsync(HttpContent content)
    {
        Uri uri = BuildUrl(_client.BaseAddress!, _endpoint).WithArguments(_arguments.ToArray()!);
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = content
        };

        return await SendAsync(request);
    }

    public async Task<TResponse> PostAsync<TResponse>(HttpContent content)
    {
        using HttpResponseMessage response = await PostAsync(content);
        TResponse? model = await response.Content.ReadAsAsync<TResponse>(Formatters, _token);
        return model;
    }
    #endregion

    #region IUploadFile
    public async Task<HttpResponseMessage> PostAsync()
    {
        Uri uri = BuildUrl(_client.BaseAddress!, _endpoint).WithArguments(_arguments.ToArray()!);
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        return await SendAsync(request);
    }
    #endregion

    #region ISetCorrelationId
    public IAssignEndpoint WithCorrelationId(string correlationId)
    {
        if (!string.IsNullOrWhiteSpace(correlationId) && !_client.DefaultRequestHeaders.Contains(Headers.CorrelationId))
        {
            _client.DefaultRequestHeaders.TryAddWithoutValidation(Headers.CorrelationId, correlationId);
        }

        return this;
    }

    public IAssignEndpoint WithGeneratedCorelationId()
    {
        string correlationId = Guid.NewGuid().ToString();
        return WithCorrelationId(correlationId);
    }
    #endregion

    #region HelperMethods
    private void SetDefaultUserAgent()
    {
        SetUserAgent($"FluentHttpClient/{version} (+http://github.com/nyronw/FluentHttpClient)");
    }

    internal static Uri BuildUrl(Uri? baseUrl, string resource)
    {
        if (Uri.TryCreate(resource, UriKind.Absolute, out Uri? absoluteUrl))
        {
            return absoluteUrl;
        }

        if (baseUrl == null)
        {
            throw new FormatException($"Can't use relative URL '{resource}' because no base URL was specified.");
        }

        if (string.IsNullOrWhiteSpace(resource))
        {
            return baseUrl;
        }

        resource = resource.Trim();
        UriBuilder builder = new(baseUrl);

        if (!string.IsNullOrWhiteSpace(builder.Fragment) || resource.StartsWith("#"))
        {
            return new Uri(baseUrl + resource);
        }

        // special case: if resource is a query string, validate and append it
        if (resource.StartsWith('?') || resource.StartsWith('&'))
        {
            bool baseHasQuery = !string.IsNullOrWhiteSpace(builder.Query);
            if (baseHasQuery && resource.StartsWith('?'))
            {
                resource = resource.Substring(1);
            }

            if (!baseHasQuery && resource.StartsWith('&'))
            {
                resource = resource.Substring(1);
            }

            return new Uri(baseUrl + resource);
        }

        // else make absolute URL
        if (!builder.Path.EndsWith('/'))
        {
            builder.Path += "/";
            baseUrl = builder.Uri;
        }

        return new Uri(baseUrl, resource);
    }

    private HttpRequestMessage BuildRequest(string endpoint, HttpMethod method, ICollection<IHttpClientFilter> filters)
        => BuildRequest<object>(endpoint, method, null, filters);

    private HttpRequestMessage BuildRequest<TBody>(string endpoint, HttpMethod method, TBody? body, ICollection<IHttpClientFilter> filters)
    {
        var model = new FluentHttpModel(_identifier, _client, _factoryProperties, _serviceProvider);
        foreach (IHttpClientFilter filter in filters)
        {
            filter.OnBeforeRequest(model);
        }

        Uri uri = BuildUrl(model.Client.BaseAddress!, endpoint).WithArguments(_arguments.ToArray()!);
        var request = new HttpRequestMessage(method, uri);

        if (body is not null)
        {
            MediaTypeFormatter formatter = Formatters.Single(f => f.SupportedMediaTypes.Any(m => m.MediaType!.Equals(_contentType)));
            var content = new ObjectContent<TBody>(body, formatter, _contentType);

            request.Content = content;
        }

        return request;
    }

    private ICollection<IHttpClientFilter> GetFilterInstances()
    {
        _filters ??= [];

        if (Filters.Count == 0)
        {
            return [];
        }

        if (_filters.Count == Filters.Count)
        {
            return _filters;
        }

        foreach (Type filter in Filters)
        {
            if (filter == null || _filters.Any(f => f.GetType() == filter) ||
                _serviceProvider.GetService(filter) is not IHttpClientFilter instance)
            {
                continue;
            }

            _filters.Add(instance);
        }

        return _filters;
    }

    private async Task<AccessToken> GetAccessToken()
    {
        AccessTokenStorage? tokenStorage = _serviceProvider.GetService<AccessTokenStorage>();

        if (tokenStorage is null)
            return AccessToken.Empty;

        AccessToken accessToken = await tokenStorage.GetToken(_identityUrl, _clientId, _clientSecret, _scopes);
        return accessToken;
    }
    #endregion
}


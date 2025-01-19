using System.Net.Http.Formatting;

namespace FluentHttpClient;

public class RequestBuilder : IRequestBuilderActions, ICreateOrSetContent,
    IRequestContentActions
{
    private readonly HttpMethod _method;
    private readonly Dictionary<string, object> _arguments = [];
    private readonly Dictionary<string, string> _headers = [];
    private string _correlationId = null!;
    private HttpContent _content = null!;
    private string _contentType = null!;

    private MediaTypeFormatterCollection Formatters { get; } = [];

    private RequestBuilder(HttpMethod method) => _method = method;

    public static IRequestBuilderActions Delete() => new RequestBuilder(HttpMethod.Delete);

    public static IRequestBuilderActions Get() => new RequestBuilder(HttpMethod.Get);

    public static IRequestBuilderActions Post() => new RequestBuilder(HttpMethod.Post);

    public static IRequestBuilderActions Put() => new RequestBuilder(HttpMethod.Put);

    public static IRequestBuilderActions UsingHttpMethod(HttpMethod method) => new RequestBuilder(method);

    public ICreateOrSetContent WithArguments(IEnumerable<KeyValuePair<string, object?>> arguments)
    {
        if (arguments == null)
            return this;

        KeyValuePair<string, object?>[] args = (
            from arg in arguments
            let key = arg.Key?.ToString()
            where !string.IsNullOrWhiteSpace(key)
            select new KeyValuePair<string, object?>(key, arg.Value)
        ).ToArray();

        _arguments.AddRange(args);

        return this;
    }

    public ICreateOrSetContent WithArguments(object? arguments)
    {
        if (arguments == null)
            return this;

        KeyValuePair<string, object?>[] args = arguments.GetKeyValueArguments().ToArray();

        _arguments.AddRange(args);

        return this;
    }

    public IRequestBuilderActions WithArgument<TArgument>(string key, TArgument value)
    {
        _arguments.Add(key, value!);
        return this;
    }

    public ICreateAction WithContent(HttpContent content)
    {
        _content = content;
        _contentType = content.Headers.ContentType!.MediaType!;

        return this;
    }

    public ICreateAction WithFormUrlEncodedContent(IEnumerable<KeyValuePair<string?, string?>> content) 
     => WithContent(new FormUrlEncodedContent(content));

    public ICreateAction WithJsonContent<TContent>(TContent content)
        => WithContent(content, "application/json");

    public ICreateAction WithXmlContent<TContent>(TContent content)
        => WithContent(content, "application/json");

    public ICreateAction WithContent<TContent>(TContent content, string contentType)
    {
        var formatter = Formatters.Single(f => f.SupportedMediaTypes.Any(m => m.MediaType!.Equals(contentType)));

        return WithContent(new ObjectContent<TContent>(content, formatter, contentType));
    }

    public IRequestBuilderActions WithHeader(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
        }

        WithHeaders(new[] { new KeyValuePair<string, string>(name, value) }!);

        return this;
    }

    public ICreateOrSetContent WithHeaders(IEnumerable<KeyValuePair<string, string>>? headers)
    {
        if (headers == null)
        {
            throw new ArgumentException($"'{nameof(headers)}' cannot be null or whitespace.", nameof(headers));
        }

        foreach (var arg in headers)
        {
            if (string.IsNullOrWhiteSpace(arg.Key)) continue;
            _headers[arg.Key] = arg.Value;
        }

        return this;
    }

    public ICreateOrSetContent WithCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    public ICreateOrSetContent WithGeneratedCorelationId()
    {
        var correlationId = Guid.NewGuid().ToString();
        return WithCorrelationId(correlationId);
    }

    public HttpRequestMessage Create()
    {
        var request = new HttpRequestMessage();
        request.Method = _method;
        request.Headers.TryAddWithoutValidation("content-type", _contentType);
        request.Headers.TryAddWithoutValidation(Headers.CorrelationId, _correlationId);

        foreach (var arg in _headers)
            request.Headers.TryAddWithoutValidation(arg.Key, arg.Value);

        request.Content = _content;

        return request;
    }
}


public interface IRequestBuilderActions : IRequestContentActions, ICreateAction
{
    IRequestBuilderActions WithHeader(string name, string value);
    IRequestBuilderActions WithArgument<TArgument>(string key, TArgument value);

    ICreateOrSetContent WithHeaders(IEnumerable<KeyValuePair<string, string>>? headers);
    ICreateOrSetContent WithArguments(object? arguments);
    ICreateOrSetContent WithArguments(IEnumerable<KeyValuePair<string, object?>> arguments);
    ICreateOrSetContent WithCorrelationId(string correlationId);
    ICreateOrSetContent WithGeneratedCorelationId();
}

public interface ICreateOrSetContent : IRequestContentActions, ICreateAction
{

}

public interface IRequestContentActions
{
    ICreateAction WithContent(HttpContent content);
    ICreateAction WithJsonContent<TContent>(TContent content);
    ICreateAction WithXmlContent<TContent>(TContent content);
    ICreateAction WithFormUrlEncodedContent(IEnumerable<KeyValuePair<string?, string?>> content);
    ICreateAction WithContent<TContent>(TContent content, string contentType);
}


public interface ICreateAction
{
    HttpRequestMessage Create();
}

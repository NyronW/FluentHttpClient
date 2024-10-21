using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace FluentHttpClient;

public static class HttpExtensions
{
    public static ICollection<HttpStatusCode> RetryableStatusCodes = new HashSet<HttpStatusCode>()
    {
        HttpStatusCode.InternalServerError,
        HttpStatusCode.GatewayTimeout,
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.BadGateway,
        HttpStatusCode.TooManyRequests
    };

    public static bool IsRetryable(this HttpStatusCode statusCode)
     => RetryableStatusCodes.Contains(statusCode);

    public static async Task<Result<TModel>> GetResultAsync<TModel>(this HttpResponseMessage response, CancellationToken token = default)
    {
        if (response == null) return new ErrorResult<TModel>("No response return from http call");

        if (response.StatusCode.IsRetryable())
        {
            return new ErrorResult<TModel>($"Received retryable status code '{response.StatusCode}'", true);
        }

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new NotFoundResult<TModel>($"Request returned a  404 not found for endpoint:{response.RequestMessage!.RequestUri!.AbsoluteUri}");

            return new ErrorResult<TModel>($"Http call failed with status code: {response.StatusCode}");
        }

        if (response.Content == null)
        {
            return new ErrorResult<TModel>("Response content was null");
        }

        var formatters = new MediaTypeFormatterCollection();
        var model = await response.Content.ReadAsAsync<TModel>(formatters, token);
        if (model == null)
        {
            return new ErrorResult<TModel>("Failed to deserialize response");
        }

        return new SuccessResult<TModel>(model);
    }

    public static async Task<HttpResponseMessage> RetryAsync(this HttpResponseMessage response, IFluentHttpClient client, CancellationToken token = default)
    {
        if (response == null) throw new ApiException(response!, "No response return from http cal");

        if (response.IsSuccessStatusCode) return response;

        var request = await response.RequestMessage!.CloneAsync();

        return await ((FluentHttpClient)client).SendAsync(request);
    }

    public static async Task<Result<TModel>> RetryResultAsync<TModel>(this HttpResponseMessage response, IFluentHttpClient client, CancellationToken token = default)
    {
        var message = await RetryAsync(response, client, token);

        return await message.GetResultAsync<TModel>();
    }

    public static HeaderValue<T> GetValue<T>(this HttpResponseHeaders headers, string name)
    {
        if (!headers.TryGetValues(name, out var rawValues)) return new();

        var value = (T)Convert.ChangeType(rawValues.First(), typeof(T));

        return new(value);
    }

    internal static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
    {
        HttpRequestMessage clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = await request.Content.CloneAsync().ConfigureAwait(false),
            Version = request.Version
        };

        clone.Options.AddRange(request.Options);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }

    internal static async Task<HttpContent?> CloneAsync(this HttpContent? content)
    {
        if (content == null)
            return null;

        Stream stream = new MemoryStream();
        await content.CopyToAsync(stream).ConfigureAwait(false);
        stream.Position = 0;

        StreamContent clone = new StreamContent(stream);
        foreach (var header in content.Headers)
            clone.Headers.Add(header.Key, header.Value);

        return clone;
    }
}

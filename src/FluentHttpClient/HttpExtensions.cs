using System.Net;
using System.Net.Http.Formatting;

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

    public static async Task<Result<TModel>> GetResult<TModel>(this HttpResponseMessage response, CancellationToken token = default)
    {
        if (response == null) return new ErrorResult<TModel>("No response return from http call");

        if (RetryableStatusCodes.Contains(response.StatusCode))
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

        var model = await response.Content.ReadAsAsync<TModel>(new MediaTypeFormatterCollection(), token);
        if (model == null)
        {
            return new ErrorResult<TModel>("Failed to deserialize response");
        }

        return new SuccessResult<TModel>(model);
    }

    public static async Task<Stream> GetStream(HttpResponseMessage response, CancellationToken token = default)
    {
        if (response == null) throw new ApiException(response!, "No response return from http cal");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApiException(response, $"Request returned a 404 not found for endpoint:{response.RequestMessage!.RequestUri!.AbsoluteUri}");

            throw new ApiException(response, $"Http call failed with status code: {response.StatusCode}");
        }

        if (response.Content == null)
        {
            throw new ApiException(response, "Response content was null");
        }

        var stream = await response.Content.ReadAsStreamAsync(token);

        return stream;
    }
}

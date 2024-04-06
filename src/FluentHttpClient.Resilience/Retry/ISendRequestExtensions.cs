using Polly.Retry;

namespace FluentHttpClient.Resilience.Retry;
public static class ISendRequestExtensions
{
    public static async Task<HttpResponseMessage> DeleteAsync(this ISendRequest client, AsyncRetryPolicy policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.DeleteAsync(httpCompletionOption));
        else
            return await client.DeleteAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this ISendRequest client, AsyncRetryPolicy<HttpResponseMessage> policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.DeleteAsync(httpCompletionOption));
        else
            return await client.DeleteAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> GetAsync(this ISendRequest client, AsyncRetryPolicy policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.GetAsync(httpCompletionOption));
        else
            return await client.GetAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> GetAsync(this ISendRequest client, AsyncRetryPolicy<HttpResponseMessage> policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.GetAsync(httpCompletionOption));
        else
            return await client.GetAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> SendAsync(this ISendRequest client, AsyncRetryPolicy policy, HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SendAsync(request, httpCompletionOption));
        else
            return await client.SendAsync(request, httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> SendAsync(this ISendRequest client, AsyncRetryPolicy<HttpResponseMessage> policy, HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SendAsync(request, httpCompletionOption));
        else
            return await client.SendAsync(request, httpCompletionOption);
    }

    public static async Task<TResponse> GetAsync<TResponse>(this ISendRequest client, AsyncRetryPolicy<TResponse> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.GetAsync<TResponse>());
        else
            return await client.GetAsync<TResponse>();
    }
}



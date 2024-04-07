using Polly.Caching;

namespace FluentHttpClient.Resilience;
public static class ISendRequestWithBodyCachingExtensions
{
    public static async Task<HttpResponseMessage> PatchAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncCachePolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PatchAsync(request));
        else
            return await client.PatchAsync(request);
    }

    public static async Task<HttpResponseMessage> PutAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncCachePolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PutAsync(request));
        else
            return await client.PutAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncCachePolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(request));
        else
            return await client.PostAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncCachePolicy<HttpResponseMessage> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(request));
        else
            return await client.PostAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync(this ISendRequestWithBody client, HttpContent content, AsyncCachePolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(content));
        else
            return await client.PostAsync(content);
    }

    public static async Task<HttpResponseMessage> PostAsync(this ISendRequestWithBody client, HttpContent content, AsyncCachePolicy<HttpResponseMessage> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(content));
        else
            return await client.PostAsync(content);
    }

    public static async Task<TResponse> PostAsync<TResponse>(this ISendRequestWithBody client, HttpContent content, AsyncCachePolicy<TResponse> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync<TResponse>(content));
        else
            return await client.PostAsync<TResponse>(content);
    }
}



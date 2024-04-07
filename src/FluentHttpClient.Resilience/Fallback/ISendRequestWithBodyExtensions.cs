using Polly.Fallback;

namespace FluentHttpClient.Resilience;

public static class ISendRequestWithBodyFallbackExtensions
{
    public static async Task<HttpResponseMessage> PatchAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncFallbackPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PatchAsync(request));
        else
            return await client.PatchAsync(request);
    }

    public static async Task<HttpResponseMessage> PutAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncFallbackPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PutAsync(request));
        else
            return await client.PutAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncFallbackPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(request));
        else
            return await client.PostAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncFallbackPolicy<HttpResponseMessage> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(request));
        else
            return await client.PostAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync(this ISendRequestWithBody client, HttpContent content, AsyncFallbackPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(content));
        else
            return await client.PostAsync(content);
    }

    public static async Task<HttpResponseMessage> PostAsync(this ISendRequestWithBody client, HttpContent content, AsyncFallbackPolicy<HttpResponseMessage> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(content));
        else
            return await client.PostAsync(content);
    }

    public static async Task<TResponse> PostAsync<TResponse>(this ISendRequestWithBody client, HttpContent content, AsyncFallbackPolicy<TResponse> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync<TResponse>(content));
        else
            return await client.PostAsync<TResponse>(content);
    }
}



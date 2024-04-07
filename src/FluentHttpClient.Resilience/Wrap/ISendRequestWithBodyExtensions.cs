using Polly.Wrap;

namespace FluentHttpClient.Resilience;
public static class ISendRequestWithBodyWrapExtensions
{
    public static async Task<HttpResponseMessage> PatchAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncPolicyWrap policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PatchAsync(request));
        else
            return await client.PatchAsync(request);
    }

    public static async Task<HttpResponseMessage> PutAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncPolicyWrap policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PutAsync(request));
        else
            return await client.PutAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncPolicyWrap policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(request));
        else
            return await client.PostAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, AsyncPolicyWrap<HttpResponseMessage> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(request));
        else
            return await client.PostAsync(request);
    }

    public static async Task<HttpResponseMessage> PostAsync(this ISendRequestWithBody client, HttpContent content, AsyncPolicyWrap policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(content));
        else
            return await client.PostAsync(content);
    }

    public static async Task<HttpResponseMessage> PostAsync(this ISendRequestWithBody client, HttpContent content, AsyncPolicyWrap<HttpResponseMessage> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync(content));
        else
            return await client.PostAsync(content);
    }

    public static async Task<TResponse> PostAsync<TResponse>(this ISendRequestWithBody client, HttpContent content, AsyncPolicyWrap<TResponse> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.PostAsync<TResponse>(content));
        else
            return await client.PostAsync<TResponse>(content);
    }
}



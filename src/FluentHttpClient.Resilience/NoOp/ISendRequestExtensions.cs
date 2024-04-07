using Polly.NoOp;

namespace FluentHttpClient.Resilience;

public static class ISendRequestNoOpExtensions
{
    public static async Task<HttpResponseMessage> DeleteAsync(this ISendRequest client, AsyncNoOpPolicy policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.DeleteAsync(httpCompletionOption));
        else
            return await client.DeleteAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this ISendRequest client, AsyncNoOpPolicy<HttpResponseMessage> policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.DeleteAsync(httpCompletionOption));
        else
            return await client.DeleteAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> GetAsync(this ISendRequest client, AsyncNoOpPolicy policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.GetAsync(httpCompletionOption));
        else
            return await client.GetAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> GetAsync(this ISendRequest client, AsyncNoOpPolicy<HttpResponseMessage> policy, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.GetAsync(httpCompletionOption));
        else
            return await client.GetAsync(httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> SendAsync(this ISendRequest client, AsyncNoOpPolicy policy, HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SendAsync(request, httpCompletionOption));
        else
            return await client.SendAsync(request, httpCompletionOption);
    }

    public static async Task<HttpResponseMessage> SendAsync(this ISendRequest client, AsyncNoOpPolicy<HttpResponseMessage> policy, HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SendAsync(request, httpCompletionOption));
        else
            return await client.SendAsync(request, httpCompletionOption);
    }

    public static async Task<TResponse> GetAsync<TResponse>(this ISendRequest client, AsyncNoOpPolicy<TResponse> policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.GetAsync<TResponse>());
        else
            return await client.GetAsync<TResponse>();
    }
}

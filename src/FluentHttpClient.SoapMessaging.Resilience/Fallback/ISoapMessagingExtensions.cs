using Polly.Fallback;

namespace FluentHttpClient.SoapMessaging.Resilience;
public static class ISendRequestFallbackExtensions
{
    public static async Task<HttpResponseMessage> SoapPostAsync(this ISendRequestWithBody client, string soapPayload, AsyncFallbackPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync(soapPayload));
        else
            return await client.SoapPostAsync(soapPayload);
    }

    public static async Task<HttpResponseMessage> SoapPostAsync<TRequest>(
        this ISendRequestWithBody client, TRequest request, AsyncFallbackPolicy policy, string methodName = "",
        string customNamespace = "")
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync(request, methodName, customNamespace));
        else
            return await client.SoapPostAsync(request, methodName, customNamespace);
    }

    public static async Task<TResponse> SoapPostAsync<TRequest, TResponse>(
        this ISendRequestWithBody client,
        TRequest request,
        AsyncFallbackPolicy policy,
        string methodName = "",
        string customNamespace = "") where TResponse : class
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync<TRequest, TResponse>(request, methodName, customNamespace));
        else
            return await client.SoapPostAsync<TRequest, TResponse>(request, methodName, customNamespace);
    }
}



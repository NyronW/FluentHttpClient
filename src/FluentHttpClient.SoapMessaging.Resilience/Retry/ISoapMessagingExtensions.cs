using Polly.Retry;

namespace FluentHttpClient.SoapMessaging.Resilience.Retry;
public static class ISendRequestRetryExtensions
{
    public static async Task<HttpResponseMessage> SoapPostAsync(this ISendRequestWithBody client, string soapPayload, AsyncRetryPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync(soapPayload));
        else
            return await client.SoapPostAsync(soapPayload);
    }

    public static async Task<HttpResponseMessage> SoapPostAsync<TRequest>(
        this ISendRequestWithBody client, TRequest request, AsyncRetryPolicy policy, string methodName = "",
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
        AsyncRetryPolicy policy,
        string methodName = "",
        string customNamespace = "") where TResponse : class
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync<TRequest, TResponse>(request, methodName, customNamespace));
        else
            return await client.SoapPostAsync<TRequest, TResponse>(request, methodName, customNamespace);
    }
}



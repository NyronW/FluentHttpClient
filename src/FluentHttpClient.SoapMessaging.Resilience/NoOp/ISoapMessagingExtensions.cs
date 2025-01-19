using Polly.NoOp;

namespace FluentHttpClient.SoapMessaging.Resilience;
public static class ISendRequestNoOpExtensions
{
    public static async Task<HttpResponseMessage> SoapPostAsync(this ISendRequestWithBody client, string soapPayload, AsyncNoOpPolicy policy)
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync(soapPayload));
        else
            return await client.SoapPostAsync(soapPayload);
    }

    public static async Task<HttpResponseMessage> SoapPostAsync<TRequest>(
        this ISendRequestWithBody client, TRequest request, AsyncNoOpPolicy policy, string methodName = "",
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
        AsyncNoOpPolicy policy,
        string methodName = "",
        string customNamespace = "") where TResponse : class
    {
        if (policy != null)
            return await policy.ExecuteAsync(() => client.SoapPostAsync<TRequest, TResponse>(request, methodName, customNamespace));
        else
            return await client.SoapPostAsync<TRequest, TResponse>(request, methodName, customNamespace);
    }
}



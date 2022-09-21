namespace FluentHttpClient;

public interface IHttpClientFilter
{
    void OnRequest(HttpRequestMessage request);

    void OnResponse(HttpResponseMessage response);
}



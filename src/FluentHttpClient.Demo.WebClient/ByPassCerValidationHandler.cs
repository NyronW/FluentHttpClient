namespace FluentHttpClient.Demo.WebClient;

public class ByPassCerValidationHandler : HttpClientHandler
{
    public ByPassCerValidationHandler()
    {
        ServerCertificateCustomValidationCallback = (httpRequestMessage, x509Certificate2, x509Chain, sslPolicyErrors) =>
        {
            return true;
        };
    }
}

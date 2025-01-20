using Microsoft.AspNetCore.Mvc;
using Wrapture;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class AbstractController(IFluentHttpClientFactory factory, ILogger<AbstractController> logger): Controller
{
    protected readonly IFluentHttpClientFactory _factory = factory;
    protected readonly ILogger<AbstractController> _logger = logger;

    protected async Task<AccessToken> GetAuthToken()
    {
        var content = new KeyValuePair<string?, string?>[]
        {
            new("client_id", "oauthClient"),
            new("client_secret", "SuperSecretPassword"),
            new("scope", "api1.read api1.write"),
            new("grant_type", "client_credentials")
        };

        HttpRequestMessage request = RequestBuilder.Post()
            .WithFormUrlEncodedContent(content)
            .Create();

        HttpResponseMessage respMsg = await _factory.Get("identity-server").Endpoint("https://localhost:7094/connect/token")
                .SendAsync(request);

        Either<HttpCallFailure, AccessToken> either = await respMsg.GetModelOrFailureAsync<AccessToken>();

        return either.Match(
            failure =>
            {
                _logger.LogWarning("Failed to get access token: {ErrorText}", failure.ErrorMessage);
                return AccessToken.Empty;
            },
            accessToken => accessToken
        );
    }
}

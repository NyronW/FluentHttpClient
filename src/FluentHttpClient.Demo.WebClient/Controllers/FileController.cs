using Microsoft.AspNetCore.Mvc;
using FluentHttpClient.AspNet;
using Wrapture;
using System.Diagnostics.CodeAnalysis;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class FileController(IFluentHttpClient<FileController> httpClient, IFluentHttpClientFactory factory,
    ILogger<FileController> logger) : Controller
{
    private readonly IFluentHttpClient<FileController> _httpClient = httpClient;
    private readonly IFluentHttpClientFactory _factory = factory;
    private readonly ILogger<FileController> _logger = logger;

    public IActionResult Index()
    {
        IFluentHttpClient unregisteredClient = _factory.Get("UnRegisteredHttpClient");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(IFormFile file)
    {
        if (file == null)
        {
            ViewData["Message"] = "Bad request: no file recieved";
            return View();
        }

        if (file.Length == 0)
        {
            ViewData["Message"] = "Bad request: invalid file recieved";
            return View();
        }

        AccessToken bearer = await GetAuthToken();
        HttpResponseMessage respMsg = await _httpClient
            .UsingBaseUrl()
            .WithHeader("x-request-client-type", "net60-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .UsingBearerToken(bearer.Token)
            .AttachFile(file)
            .PostAsync();

        HeaderValue<string> result = respMsg.Headers.GetValue<string>("x-file-name");

        ViewData["Message"] = respMsg.IsSuccessStatusCode ?
            "File uploaded successfully" + (result.HasValue ? $" saved as {result.Value}" : "") : "File was not uploaded";

        return View();
    }

    private async Task<AccessToken> GetAuthToken()
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

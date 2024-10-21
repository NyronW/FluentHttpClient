using Microsoft.AspNetCore.Mvc;
using FluentHttpClient.AspNet;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class FileController : Controller
{
    private readonly IFluentHttpClient<FileController> _httpClient;
    private readonly IFluentHttpClientFactory _factory;

    public FileController(IFluentHttpClient<FileController> httpClient, IFluentHttpClientFactory factory)
    {
        _httpClient = httpClient;
        _factory = factory;
    }

    public IActionResult Index()
    {
        var unregisteredClient = _factory.Get("UnRegisteredHttpClient");

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

        var bearer = await GetAuthToken();
        var respMsg = await _httpClient
            .UsingBaseUrl()
            .WithHeader("x-request-client-type", "net60-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .UsingBearerToken(bearer.Token)
            .AttachFile(file)
            .PostAsync();

        var result = respMsg.Headers.GetValue<string>("x-file-name");

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

        var request = RequestBuilder.Post()
            .WithFormUrlEncodedContent(content)
            .Create();

        var respMsg = await _factory.Get("identity-server").Endpoint("https://localhost:7094/connect/token")
                .SendAsync(request);

        var result = await respMsg.GetResultAsync<AccessToken>();

        return result.Data;
    }
}

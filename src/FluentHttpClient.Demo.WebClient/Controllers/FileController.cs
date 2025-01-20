using Microsoft.AspNetCore.Mvc;
using FluentHttpClient.AspNet;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class FileController(IFluentHttpClient<FileController> httpClient, IFluentHttpClientFactory factory,
    ILogger<FileController> logger) : AbstractController(factory, logger)
{
    private readonly IFluentHttpClient<FileController> _httpClient = httpClient;

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
            .WithHeader("x-request-client-type", "net8.0-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .UsingBearerToken(bearer.Token)
            .AttachFile(file)
            .PostAsync();

        HeaderValue<string> result = respMsg.Headers.GetValue<string>("x-file-name");

        ViewData["Message"] = respMsg.IsSuccessStatusCode ?
            "File uploaded successfully" + (result.HasValue ? $" saved as {result.Value}" : "") : "File was not uploaded";

        return View();
    }
}

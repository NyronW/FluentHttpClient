using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class FileController : Controller
{
    private readonly IFluentHttpClientFactory _httpClientFactory;

    public FileController(IFluentHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
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

        var client = _httpClientFactory.Get("file-upload");

        var respMsg = await client
            .Endpoint("/api/v1/files")
            .WithHeader("x-request-client-type", "net60-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .AttachFile(file)
            .PostAsync();

        ViewData["Message"] = respMsg.IsSuccessStatusCode ?
            "File uploaded successfully" : "File was not uploaded";

        return View();
    }
}

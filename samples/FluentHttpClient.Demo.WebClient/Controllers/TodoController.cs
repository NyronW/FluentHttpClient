using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Polly;
using Polly.Retry;
using FluentHttpClient.Resilience;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class TodoController : Controller
{
    private readonly IFluentHttpClient _fluentHttpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .Handle<HttpRequestException>() // Specify the exceptions on which to retry.
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Optionally, retry on unsuccessful HTTP response codes.
            .WaitAndRetryAsync(3, retryAttempt =>
                 TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential back-off. e.g., 2s, 4s, 8s.
            );

    public TodoController(IFluentHttpClient<TodoController> fluentHttpClient)
    {
        _fluentHttpClient = fluentHttpClient;
    }

    public async Task<IActionResult> Index(int pageNo = 1, int pageSize = 10)
    {
        var args = new Dictionary<string, object>
        (
            [
                new KeyValuePair<string, object>("pageNo", pageNo),
                new KeyValuePair<string, object>("pageSize", pageSize)
            ]
        );

        using var response = await _fluentHttpClient
            .Endpoint("todos")
            //.WithArguments(new { pageNo = pageNo, pageSize = pageSize })
            .WithArguments(args)
            .WithGeneratedCorelationId()
            .UsingIdentityServer("https://localhost:7094", "oauthClient", "SuperSecretPassword", "api1.read")
            .GetAsync();

        //todo:implement client side streams
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        var options = new JsonSerializerOptions
        {
            DefaultBufferSize = 10, // Adjust as necessary
            PropertyNameCaseInsensitive = true // If your JSON properties are not case-sensitive
        };
        
        var data = new List<TodoItem>();

        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<TodoItem>(stream, options))
        {
            data.Add(item);
        }

        var totalItems = response.Headers.GetValue<int>("x-total-items");

        if (totalItems.HasValue) ViewData["TotalItems"] = totalItems.Value;

        return View(data);
    }

    public IActionResult Create()
    {
        return View(new TodoItemDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoItemDto todoItem)
    {
        if (todoItem == null)
            return View();

        var cts = new CancellationTokenSource();

        var respMsg = await _fluentHttpClient
            .Endpoint("todos")
            .WithHeader("x-request-client-type", "net60-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .UsingJsonFormat()
            .WithCancellationToken(cts.Token)
            .UsingIdentityServer("https://localhost:7094", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
            .PostAsync(todoItem);

        var result = await respMsg.GetResultAsync<TodoItem>();

        if (result.Failure)
        {
            if (result.Retryable)
            {
                result = await respMsg.RetryResultAsync<TodoItem>(_fluentHttpClient);

                if (result.Success)
                {
                    TempData["Message"] = $"Todo item created with id:{result.Data.Id}";
                    return RedirectToAction("Index");
                }
            }

            ViewData["ErrorMessage"] = respMsg.ReasonPhrase;

            return View(todoItem);
        }

        TempData["Message"] = $"Todo item created with id:{result.Data.Id}";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(string id)
    {
        var response = await _fluentHttpClient
            .Endpoint($"todos/{id}")
            .WithCorrelationId("R5cCI6IkpXVCJ9.get")
            .UsingXmlFormat()
            .UsingIdentityServer("https://localhost:7094", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
            .GetAsync(retryPolicy);

        var result = await response.GetResultAsync<TodoItem>();

        if (result.Failure)
        {
            TempData["Message"] = result switch
            {
                NotFoundResult<TodoItem> notFoundResult => notFoundResult.Message,
                ErrorResult<TodoItem> errorResult => errorResult.Message,
                _ => "Unhandled error occured while searching for item"
            };

            return RedirectToAction("Index");
        }

        var item = result.Data;

        return View(item);
    }
}

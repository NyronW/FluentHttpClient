using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Polly;
using Polly.Retry;
using FluentHttpClient.Resilience;
using Wrapture;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class TodoController(
    IFluentHttpClient<TodoController> fluentHttpClient,
    IFluentHttpClientFactory factory,
    ILogger<TodoController> logger) : AbstractController(factory, logger)
{
    private readonly IFluentHttpClient _fluentHttpClient = fluentHttpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .Handle<HttpRequestException>() // Specify the exceptions on which to retry.
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Optionally, retry on unsuccessful HTTP response codes.
            .WaitAndRetryAsync(3, retryAttempt =>
                 TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential back-off. e.g., 2s, 4s, 8s.
            );

    //public async Task<IActionResult> Index(int pageNo = 1, int pageSize = 10)
    public async Task<IActionResult> Index()
    {
        //var args = new Dictionary<string, object>
        //(
        //    [
        //        new KeyValuePair<string, object>("pageNo", pageNo),
        //        new KeyValuePair<string, object>("pageSize", pageSize)
        //    ]
        //);

        //using HttpResponseMessage response = await _fluentHttpClient
        //    .Endpoint("todos")
        //    //.WithArguments(new { pageNo = pageNo, pageSize = pageSize })
        //    .WithArguments(args)
        //    .WithGeneratedCorelationId()
        //    .UsingIdentityServer("https://localhost:7094", "oauthClient", "SuperSecretPassword", "api1.read")
        //    .GetAsync();

        //response.EnsureSuccessStatusCode();

        //Stream stream = await response.Content.ReadAsStreamAsync();

        //var options = new JsonSerializerOptions
        //{
        //    DefaultBufferSize = 10, // Adjust as necessary
        //    PropertyNameCaseInsensitive = true // If your JSON properties are not case-sensitive
        //};

        //var data = new List<TodoItem>();

        //await foreach (TodoItem? item in JsonSerializer.DeserializeAsyncEnumerable<TodoItem>(stream, options))
        //{
        //    if (item is null)
        //    {
        //        continue;
        //    }

        //    data.Add(item);
        //}

        //HeaderValue<int> totalItems = response.Headers.GetValue<int>("x-total-items");

        //if (totalItems.HasValue)
        //{
        //    ViewData["TotalItems"] = totalItems.Value;
        //}

        //return View(data);

        AccessToken token = await GetAuthToken();

        ViewData["ApiUrl"] = _fluentHttpClient
            .Endpoint("todos").GetRequestUrl().AbsoluteUri;

        return View(token );
    }

    public IActionResult Create() => View(new TodoItemDto());

    [HttpPost]
    public async Task<IActionResult> Create(TodoItemDto todoItem)
    {
        if (todoItem == null)
        {
            return View();
        }

        using var cts = new CancellationTokenSource();

        HttpResponseMessage respMsg = await _fluentHttpClient
            .Endpoint("todos")
            .WithHeader("x-request-client-type", "net8.0-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .UsingJsonFormat()
            .WithCancellationToken(cts.Token)
            .UsingIdentityServer("https://localhost:7094", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
            .PostAsync(todoItem);

        Either<HttpCallFailure, TodoItem> either = await respMsg.GetModelOrFailureAsync<TodoItem>();

        return await either.MatchAsync<IActionResult>(
            async failure =>
            {
                string? errorText = failure.ErrorMessage ?? respMsg.ReasonPhrase;
                if (failure.IsRetryable)
                {
                    var retryResult = await respMsg.RetryResultAsync<TodoItem>(_fluentHttpClient);
                    if (retryResult.IsSuccess)
                    {
                        TempData["Message"] = $"Todo item created with id: {retryResult.Value.Id}";
                        return RedirectToAction("Index");
                    }

                    errorText = retryResult.Error;
                }

                ViewData["ErrorMessage"] = errorText;
                return View(todoItem);
            },
            async success =>
            {
                await Task.Yield();
                TempData["Message"] = $"Todo item created with id: {success.Id}";
                return RedirectToAction("Index");
            }
        );
    }

    public async Task<IActionResult> Details(string id)
    {
        HttpResponseMessage response = await _fluentHttpClient
            .Endpoint($"todos/{id}")
            .WithCorrelationId("R5cCI6IkpXVCJ9.get")
            .UsingXmlFormat()
            .UsingIdentityServer("https://localhost:7094", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
            .GetAsync(retryPolicy);

        var result = await response.GetResultAsync<TodoItem>();

        if (result.IsFailure)
        {
            TempData["Message"] = result switch
            {
                NotFoundResult<TodoItem> notFoundResult => notFoundResult.Error,
                ErrorResult<TodoItem> errorResult => errorResult.Error,
                _ => "Unhandled error occured while searching for item"
            };

            return RedirectToAction("Index");
        }

        var item = result;

        return View(item);
    }
}

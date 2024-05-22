using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Polly;
using Polly.Retry;
using FluentHttpClient.Resilience;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class TodoController : Controller
{
    private readonly IFluentHttpClientFactory _clientFactory;

    private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .Handle<HttpRequestException>() // Specify the exceptions on which to retry.
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Optionally, retry on unsuccessful HTTP response codes.
            .WaitAndRetryAsync(3, retryAttempt =>
                 TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential back-off. e.g., 2s, 4s, 8s.
            );

    public TodoController(IFluentHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> Index(int pageNo = 1, int pageSize = 10)
    {
        var client = _clientFactory.Get<TodoController>();
        var bearer = await GetAuthToken();

        var args = new Dictionary<string, object>
        (
            [
                new KeyValuePair<string, object>("pageNo", pageNo),
                new KeyValuePair<string, object>("pageSize", pageSize)
            ]
        );

        using var response = await client
            .Endpoint("todos")
            //.WithArguments(new { pageNo = pageNo, pageSize = pageSize })
            .WithArguments(args)
            .WithGeneratedCorelationId()
            .UsingBearerToken(bearer.Token)
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

        var client = _clientFactory.Get<TodoController>();
        var cts = new CancellationTokenSource();

        var respMsg = await client
            .Endpoint("/todos")
            .WithHeader("x-request-client-type", "net60-aspnet")
            .WithCorrelationId("R5cCI6IkpXVCJ9.post")
            .UsingJsonFormat()
            .WithCancellationToken(cts.Token)
            .UsingIdentityServer("https://localhost:7094/connect/token", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
            .PostAsync(todoItem);

        var result = await respMsg.GetResultAsync<TodoItem>();

        if (result.Failure)
        {
            if (result.Retryable)
            {
                result = await respMsg.RetryResultAsync<TodoItem>(client);

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
        var response = await _clientFactory.Get<TodoController>()
            .Endpoint($"/api/v1/todos/{id}")
            .WithCorrelationId("R5cCI6IkpXVCJ9.get")
            .UsingXmlFormat()
            .UsingIdentityServer("https://localhost:7094/connect/token", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
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

    private async Task<AccessToken> GetAuthToken()
    {
        var content = new FormUrlEncodedContent(new KeyValuePair<string?, string?>[]
        {
            new("client_id", "oauthClient"),
            new("client_secret", "SuperSecretPassword"),
            new("scope", "api1.read api1.write"),
            new("grant_type", "client_credentials")
        });


        var policy = Policy
            .Handle<HttpRequestException>() // Specify the exceptions on which to retry.
            .OrResult<AccessToken>(a => string.IsNullOrWhiteSpace(a.Token)) // Optionally, retry on unsuccessful HTTP response codes.
            .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Exponential back-off. e.g., 2s, 4s, 8s.
            );


        var circuitBreakerPolicy = Policy<AccessToken>
            .HandleResult(a => string.IsNullOrWhiteSpace(a.Token)) // Assuming empty access token indicates a need to break the circuit
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        var timeoutPolicy = Policy
             .TimeoutAsync<AccessToken>(TimeSpan.FromSeconds(10));

        var fallbackPolicy = Policy<AccessToken>
            .Handle<HttpRequestException>()
            .FallbackAsync(fallbackValue: new AccessToken("", 00)!);

        var bulkheadPolicy = Policy
            .BulkheadAsync<AccessToken>(maxParallelization: 3, maxQueuingActions: 6);

        var policyWrap = policy.WrapAsync(circuitBreakerPolicy);


        var noOpPolicy = Policy.NoOpAsync<AccessToken>();


        var authToken = await _clientFactory.Get("identity-server").Endpoint("https://localhost:7094/connect/token")
                .PostAsync(content, noOpPolicy);

        return authToken;
    }
}

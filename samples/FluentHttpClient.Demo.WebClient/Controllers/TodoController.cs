using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class TodoController : Controller
{
    private readonly IFluentHttpClientFactory _clientFactory;

    public TodoController(IFluentHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> Index(int pageNo = 1, int pageSize = 10)
    {
        var client = _clientFactory.Get<TodoController>();
        var bearer = await GetAuthToken();

        var response = await client
          .Endpoint("/api/v1/todos")
          .WithArguments(new { pageNo = pageNo, pageSize = pageSize })
          .WithGeneratedCorelationId()
          .UsingBearerToken(bearer.Token)
          .GetAsync();

        var result = await response.GetResultAsync<TodoItem[]>();
        var totalItems = response.Headers.GetValue<int>("x-total-items");

        if (totalItems.HasValue) ViewData["TotalItems"] = totalItems.Value;

        return View(result.Data);
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
            .Endpoint("/api/v1/todos")
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
            .GetAsync();

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

        var authToken = await _clientFactory.Get("identity-server").Endpoint("https://localhost:7094/connect/token")
            .PostAsync<AccessToken>(content);

        return authToken;
    }
}

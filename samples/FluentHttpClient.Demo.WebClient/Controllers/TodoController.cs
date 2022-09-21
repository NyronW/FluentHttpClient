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

        var items = await client
          .Endpoint("/api/v1/todos")
          .WithArguments(new { pageNo = pageNo, pageSize = pageSize })
          .WithGeneratedCorelationId()
          .UsingBearerToken(bearer.Token)
          .GetAsync<TodoItem[]>();

        return View(items);
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

        var result = await respMsg.GetResult<TodoItem>();

        if (result.Failure)
        {
            if (result.Retryable)
            {
                ViewData["Message"] = "Retryable error received";
            }

            return View(todoItem);
        }

        TempData["Message"] = $"Todo item created with id:{result.Data.Id}";

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(string id)
    {
        var item = await _clientFactory.Get<TodoController>()
            .Endpoint($"/api/v1/todos/{id}")
            .WithCorrelationId("R5cCI6IkpXVCJ9.get")
            .UsingXmlFormat()
            .UsingIdentityServer("https://localhost:7094/connect/token", "oauthClient", "SuperSecretPassword", "api1.read", "api1.write")
            .GetAsync<TodoItem>();

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

# YAFLHttp
Yet Another Fluent Library for Http client (YAFLHttp) is a fluent API for working with HTTP client class that seeks to provide a simply 
and intuitive devloper experience. Its fluent interface allows you send an HTTP request and parse the response by hiding away the details such as 
deserialisation, content negotiation, and URL encoding:

### Installing YAFLHttp

You should install [YAFLHttp with NuGet](https://www.nuget.org/packages/YAFLHttp):

    Install-Package YAFLHttp
    
Or via the .NET command line interface (.NET CLI):

    dotnet add package YAFLHttp

Either commands, from Package Manager Console or .NET Core CLI, will allow download and installation of YAFLHttp and all its required dependencies.

### How do I get started?

First, configure YAFLHttp by adding required http clients, in the startup of your application:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentHttp("identity-server", builder =>
{
    builder.WithTimeout(10);
}).AddFluentHttpClientFilter<MyCustomClientFilter>() //runs for all request
.AddFluentHttp<TodoController>(builder =>
 {
     builder.WithBaseUrl("https://localhost:18963/")
         .WithHeader("x-api-version", "1.0.0-beta")
         .AddFilter<TimerHttpClientFilter>() //runs only for this client
         .WithTimeout(20)
         .Register();
 }).AddFluentHttp("file-upload", builder =>
 {
     builder.WithBaseUrl("https://localhost:18963/")
        .WithTimeout(TimeSpan.FromMinutes(2));
 });
```

Inject the IFluentHttpClientFactory where you need to work with an HttpClient instance.

```csharp
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
```

You can also inject a typed client in the consuming class constuctor as follows:

```csharp
public class TodoController : Controller
{
    private readonly IFluentHttpClient _fluentHttpClient;

    public TodoController(IFluentHttpClient<TodoController> fluentHttpClient)
    {
        _fluentHttpClient = fluentHttpClient;
    }

    public async Task<IActionResult> Index(int pageNo = 1, int pageSize = 10)
    {
        var items = await _fluentHttpClient
          .Endpoint("/api/v1/todos")
          .WithArguments(new { pageNo = pageNo, pageSize = pageSize })
          .WithGeneratedCorelationId()
          .GetAsync<TodoItem[]>();

        return View(items);
    }
}
```

The code above will inject the client that was registered using generic method AddFluentHttp<TConsumer> during application startup

### Register a fluent http client

You can sent a few properties on the http client during registration such as base url, http headers, request time and http filters.
Http clients can be register by name or strongly type approach which makes selecting the correct client even easier.

```csharp

builder.Services.AddFluentHttp("identity-server", builder =>
{
    builder.WithTimeout(10);
})

builder.Services.AddFluentHttp<TodoController>(builder =>
 {
     builder.WithBaseUrl("https://localhost:18963/")
         .WithHeader("x-api-version", "1.0.0-beta")
         .AddFilter<TimerHttpClientFilter>()
         .WithTimeout(20)
         .Register();
 })
```

When registering the fluent http client the Register method must be called to complete the process, however the library will automaticaly call the method if it is not explicitly called by your code.

### Working with a fluent http client

To get and instance of a registerd fluent http client, you simply need to inject the IFluentHttpClientFactory and call its Get method using the named or strongly typed version.

```csharp
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

       //...
       
    }
 }
```
You can set request properties such as http headers, querystrings, correlation Id, content type,authentication scheme and attach files for upload to a remote server.

```csharp
var client = _httpClientFactory.Get("file-upload");

var respMsg = await client
    .Endpoint("/api/v1/files")
    .WithHeader("x-request-client-type", "net60-aspnet")
    .WithCorrelationId("R5cCI6IkpXVCJ9.post")
    .AttachFile(file)
    .PostAsync();
```

The library has built in support for work with oauth2 client credentials flow which makes it much easier to integrate with many REST based APIs.  You can also used bearer token or basic authentication when communicating with APIs.

```csharp
//...

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

//..
```
#### Retrying failed requests
 You are able to check if a failed request can be retried and resend the request with relative ease.
 
 ```csharp
 //..
 
 var respMsg = await client
    .Endpoint("/api/v1/todos")
    .WithHeader("x-request-client-type", "net60-aspnet")
    .WithCorrelationId("R5cCI6IkpXVCJ9.post")
    .UsingJsonFormat()
    .PostAsync(todoItem);
            
    var retryable = respMsg.StatusCode.IsRetryable();
    if(retryable)
    {
        //do some stuff
    }

    //or use the request objects to test if request is retryable

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

```
 You can also handle http response in a functional manner using the GetModelOrFailureAsync method which returns an [Either](https://www.nuget.org/packages/YAFLHttp) instance that can two possible outcomes such as
 HttpCallFailure or your custom model.


 ```csharp
 //..
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
 ```

 ### Supporting Packages
 
 YAFLHttp have the following supporting packages to that you can add to your project to gain additional functionality:

 * YAFLHttp.Resilience - Uses Polly to facilitates various resiliency functions such as retry , circuit breaker etc. This package offer overloads of the existing method ( Get, Post etc) that accepts a specific resilence policy.
 * YAFLHttp.SoapMessaging - Adds support for interacting with soap services in a fluent manner by adding three new methods to send soap strings or serializable objects to soap endpoints.
 * YAFLHttp.SoapMessaging.Resilience - Adds resilience to the YAFLHttp.SoapMessaging package.
 * YAFLHttp.AspNet - Adds useful methods that are needed to web applications such as file updloads and getting correlation id from http header.
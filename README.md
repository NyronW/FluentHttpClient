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
}).AddFluentHttp<TodoController>(builder =>
 {
     builder.WithBaseUrl("https://localhost:18963/")
         .WithHeader("x-api-version", "1.0.0-beta")
         .AddFilter<TimerHttpClientFilter>()
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

# YAFLHttp
Yet Another Fluent Library for Http client (YAFLHttp) is a fluent API for working with HTTP client class that seeks to provide a simply 
and intuitive devloper experience

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
//...

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
//...

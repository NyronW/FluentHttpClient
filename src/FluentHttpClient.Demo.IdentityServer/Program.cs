using FluentHttpClient.Demo.IdentityServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServer()
 .AddInMemoryClients(Clients.Get())
 .AddInMemoryIdentityResources(Resources.GetIdentityResources())
 .AddInMemoryApiResources(Resources.GetApiResources())
 .AddInMemoryApiScopes(Resources.GetApiScopes())
 .AddDeveloperSigningCredential();

var app = builder.Build();

app.UseIdentityServer();

app.MapGet("/", () => "Hello Identity Server!");

app.Run();

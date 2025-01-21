using FluentHttpClient.Demo.IdentityServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // Recommended for production
    options.EmitStaticAudienceClaim = true;

    // Configure your issuer URI
    options.IssuerUri = "https://localhost:7094";
})
    .AddInMemoryIdentityResources(Resources.GetIdentityResources())
    .AddInMemoryApiScopes(Resources.GetApiScopes())
    .AddInMemoryApiResources(Resources.GetApiResources())
    .AddInMemoryClients(Clients.Get())
    // Add development signing credential - Replace with proper cert in production
    .AddDeveloperSigningCredential();

// Add CORS
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("https://localhost:7067") // Your client app URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

var app = builder.Build();

app.UseIdentityServer();

app.MapGet("/", () => "Hello Identity Server!");

app.Run();

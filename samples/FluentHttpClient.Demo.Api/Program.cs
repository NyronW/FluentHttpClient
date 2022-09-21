using FluentHttpClient.Demo.Api;

var builder = WebApplication.CreateBuilder(args)
        .ConfigureBuilder();

var app = builder.Build().ConfigureApplication();

app.Run();

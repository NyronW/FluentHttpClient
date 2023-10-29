using FluentHttpClient;
using FluentHttpClient.Demo.WebClient;
using FluentHttpClient.Demo.WebClient.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


var configureHandler = () =>
{
    var bypassCertValidation = builder.Configuration.GetValue<bool>("BypassRemoteCertificateValidation");
    var handler = new HttpClientHandler();
    //!DO NOT DO IT IN PRODUCTION!! GO AND CREATE VALID CERTIFICATE!

    if (bypassCertValidation)
    {
        handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, x509Certificate2, x509Chain, sslPolicyErrors) =>
        {
            return true;
        };
    }
    return handler;
};

builder.Services.AddTransient<ByPassCerValidationHandler>();

builder.Services.AddFluentHttp("identity-server", builder =>
{
    builder.WithTimeout(10)
    .WithHandler(configureHandler);
}).AddFluentHttp<TodoController>(builder =>
 {
     builder.WithBaseUrl("https://localhost:18963/")
         .WithHeader("x-api-version", "1.0.0-beta")
         .AddFilter<TimerHttpClientFilter>()
         .WithHandler<ByPassCerValidationHandler>()
         //.WithTimeout(20)
         .Register();
 }).AddFluentHttp("file-upload", builder =>
 {
     builder.WithBaseUrl("https://localhost:18963/api/v1/files")
        .WithTimeout(TimeSpan.FromMinutes(2));
 });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using FluentHttpClient;
using FluentHttpClient.AspNet;
using FluentHttpClient.Demo.WebClient;
using FluentHttpClient.Demo.WebClient.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddHttpContextAccessor();

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
    }).AddFluentHttpClientFilter<TimerHttpClientFilter>()
    .AddFluentHttp<TodoController>(builder =>
     {
         builder.WithBaseUrl("https://localhost:18963/api/v1")
             .WithHeader("x-api-version", "1.0.0-beta")
             .WithTimeout(10)
             .WithHandler<ByPassCerValidationHandler>()
             .Register();
     })
    .AddFluentHttp<FileController>(builder =>
     {
         builder.WithBaseUrl("https://localhost:18963/api/v1/files")
            .WithTimeout(TimeSpan.FromMinutes(2));
     })
    .AddFluentHttp("soap", builder =>
    {
        builder.WithBaseUrl("http://www.dneonline.com/calculator.asmx")
           .WithTimeout(TimeSpan.FromMinutes(2));
    })
    .AddFluentHttp("data-flex", builder =>
    {
        builder.WithBaseUrl("http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso");
    })
    .AddFluentHttp("localhost", (sp, builder) =>
     {
         builder.ForInternalApis()
            .WithTimeout(TimeSpan.FromSeconds(10));
     })
    .AddFluentHttpAstNetCore() //Needed to support internal(localhost) api
    .AddFluentHttp("absolute", (sp, builder) =>
    {
        builder.WithTimeout(TimeSpan.FromSeconds(10));
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

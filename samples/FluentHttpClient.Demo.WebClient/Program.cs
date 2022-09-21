using FluentHttpClient;
using FluentHttpClient.Demo.WebClient.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

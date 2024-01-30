using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FluentHttpClient.Demo.WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFluentHttpClientFactory _clientFactory;

        public HomeController(IFluentHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.Get("localhost");

            var resp = await client.Endpoint("/api/values")
                .WithArgument("foo","bar")
                .GetAsync();

            var msg = await resp.Content.ReadAsStringAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
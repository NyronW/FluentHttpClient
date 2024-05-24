using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FluentHttpClient.SoapMessaging;
using System.Xml.Serialization;
using System.Xml;

namespace FluentHttpClient.Demo.WebClient.Controllers;

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
            .WithArgument("foo", "bar")
            .GetAsync();

        var msg = await resp.Content.ReadAsStringAsync();

        return View();
    }

    public async Task<IActionResult> Soap()
    {
        var rand = new Random();
        var client = _clientFactory.Get("soap");
        var intA = rand.Next(6, 10);
        var intB = rand.Next(1, 5);
        //return the HttpMessage to manually parse response
        //var response = await client.UsingBaseUrl()
        //    .UsingBasicAuthentication("", "")
        //    .SoapPostAsync(new AddRequest(intA, intB), "Add", "http://tempuri.org/");
        //string content = await response.Content.ReadAsStringAsync();

        //returning strongly typed response
        var addition = await client.UsingBaseUrl()
          .SoapPostAsync<AddRequest, AddResponse>(new AddRequest(intA, intB), "Add", "http://tempuri.org/");

        var multiplication = await client.UsingBaseUrl()
            .SoapPostAsync<MultiplyRequest, MultiplyResponse>(new MultiplyRequest(intA, intB));

        var url = client.GetBaseUrl();

        var client2 = _clientFactory.Get("data-flex");
        var responseMessage = await client2.UsingBaseUrl().SoapPostAsync<ListOfContinents>(new());
        var continents = await responseMessage.Content.ReadAsStringAsync();

        return View(new SoapViewModel { SoapServiceUrl = url, intA = intA, intB = intB, AdditionResult = addition.AddResult, MultiplicationResult = multiplication.MultiplyResult });
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


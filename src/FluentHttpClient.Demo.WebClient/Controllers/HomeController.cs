using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FluentHttpClient.SoapMessaging;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;

namespace FluentHttpClient.Demo.WebClient.Controllers;

public class HomeController(IFluentHttpClientFactory clientFactory) : Controller
{
    private readonly IFluentHttpClientFactory _clientFactory = clientFactory;

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
        IFluentHttpClient client = _clientFactory.Get("soap");
        int intA = rand.Next(6, 10);
        int intB = rand.Next(1, 5);

        //return the HttpMessage to manually parse response
        HttpResponseMessage response = await client.UsingBaseUrl()
            .UsingBasicAuthentication("", "")
            .SoapPostAsync(new AddRequest(intA, intB), "Add", "http://tempuri.org/");

        _ = await response.Content.ReadAsStringAsync();

        //returning strongly typed response
        AddResponse addition = await client.UsingBaseUrl()
          .SoapPostAsync<AddRequest, AddResponse>(new AddRequest(intA, intB), "Add", "http://tempuri.org/");

        MultiplyResponse multiplication = await client.UsingBaseUrl()
            .SoapPostAsync<MultiplyRequest, MultiplyResponse>(new MultiplyRequest(intA, intB));

        string? url = client.BaseUrl?.AbsoluteUri;

        IFluentHttpClient client2 = _clientFactory.Get("data-flex");

        _ = await client2.UsingBaseUrl().SoapPostAsync<ListOfContinents, ListOfContinentsByNameResponse>(new());


        return View(new SoapViewModel { SoapServiceUrl = url!, intA = intA, intB = intB, AdditionResult = addition.AddResult, MultiplicationResult = multiplication.MultiplyResult });
    }

    public async Task<IActionResult> Privacy()
    {
        var client = _clientFactory.Get("absolute");

        HttpResponseMessage resp = await client.Endpoint("https://cat-fact.herokuapp.com/facts/")
                        .GetAsync();

        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadAsStringAsync();

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

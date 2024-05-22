using FluentHttpClient.Demo.WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FluentHttpClient.SoapMessaging;
using System.Xml.Serialization;
using System.Xml;

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
                .WithArgument("foo", "bar")
                .GetAsync();

            var msg = await resp.Content.ReadAsStringAsync();

            return View();
        }

        public async Task<IActionResult> Soap()
        {
            var client = _clientFactory.Get("soap");

            var request = new SoapRequest(1, 2);

            var response = await client.UsingBaseUrl()
                .SoapPostAsync(request, "Add", "http://tempuri.org/");

            string content = await response.Content.ReadAsStringAsync();
            //var xmlSerializer = new XmlSerializer(typeof(SoapEnvelopeResponse<AddResponse>));
            //var resp = (SoapEnvelopeResponse<AddResponse>)xmlSerializer.Deserialize(XmlReader.Create(new StringReader(content)))!;

            var result = await client.UsingBaseUrl()
              .SoapPostAsync<SoapRequest, AddResponse>(request, "Add", "http://tempuri.org/", "AddResponse");

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

        public class SoapRequest
        {
            public SoapRequest()
            {

            }

            public SoapRequest(int inta, int intb)
            {
                intA = inta;
                intB = intb;
            }

            public int intA { get; set; }
            public int intB { get; set; }
        }

        [XmlRoot("AddResponse", Namespace = "http://tempuri.org/")]
        public class AddResponse: ISoapBody
        {
            public AddResponse()
            {

            }

            [XmlElement]
            public int AddResult { get; set; }
        }
    }
}
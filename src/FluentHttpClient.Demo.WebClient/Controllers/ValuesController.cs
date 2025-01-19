using Microsoft.AspNetCore.Mvc;

namespace FluentHttpClient.Demo.WebClient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult LocalApiTest(string foo)
        {
            return Ok($"Hello world: {foo}");
        }
    }
}

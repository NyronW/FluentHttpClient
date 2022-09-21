using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FluentHttpClient;

public class TimerHttpClientFilter : IHttpClientFilter
{
    private readonly Stopwatch _stopWatch;
    private readonly ILogger<TimerHttpClientFilter> _logger;

    public TimerHttpClientFilter(ILogger<TimerHttpClientFilter> logger)
    {
        _stopWatch = new Stopwatch();
        _logger = logger;
    }

    public void OnRequest(HttpRequestMessage request)
    {
        _stopWatch.Restart();
    }

    public void OnResponse(HttpResponseMessage response)
    {
        _stopWatch.Stop();
        var elapsedTime = _stopWatch.ElapsedMilliseconds;
        var request = response.RequestMessage!;

        _logger.LogInformation("{HttpMethod} request to {RequestUrl} completed with status {HttpStatus} in {HttpCallElapsedTime} ms", request.Method.Method, request.RequestUri.AbsoluteUri, response.StatusCode, elapsedTime);
    }
}

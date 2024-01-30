using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentHttpClient;

public static class FluentHttpClientExtensions
{
    public static IAssignEndpoint WithHttpContextCorrelationId(this IAssignEndpoint client, IHttpContextAccessor httpContextAccessor)
    {
        if (!httpContextAccessor.HttpContext.Request.Headers.TryGetValue(
                Headers.CorrelationId,
                out var headerValues))
        {
            return client;
        }

        if (string.IsNullOrWhiteSpace(headerValues))
        {
            return client;
        }

        var correlationId = headerValues.First();
        return client.WithCorrelationId(correlationId);
    }

    public static ISendFileActions AttachFile(this IAttachFiles attachFiles, IFormFile file)
    {
        return attachFiles.AttachFile(file.FileName, file.OpenReadStream());
    }

    public static ISendFileActions AttachFiles(this IAttachFiles attachFiles, IFormFile[] files)
    {
        var streams = files.Select(f => new KeyValuePair<string, Stream>(f.Name, f.OpenReadStream()));

        foreach (var kvp in streams)
            attachFiles.AttachFile(kvp.Key, kvp.Value);

        return (ISendFileActions)attachFiles;
    }

    public static ISetDefaultHeader ForInternalApis(this IFluentClientBuilderAction builderAction)
    {
        builderAction.AddFilter<InternalApiUrlSetterHttpClientFilter>();
        builderAction.WithProperty("InternalApis", true);

        return builderAction;
    }

    public static IServiceCollection AddFluentHttpAstNetCore(this IServiceCollection services)
    {
        services.AddFluentHttp();

        services.AddTransient<InternalApiUrlSetterHttpClientFilter>();

        return services;
    }

    internal class InternalApiUrlSetterHttpClientFilter : IHttpClientFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public InternalApiUrlSetterHttpClientFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnBeforeRequest(FluentHttpModel model)
        {
            if (!model.Properties.TryGetValue("InternalApis", out object? localApi)) return;
            if (localApi is not true) return;

            var accessor = _serviceProvider.GetService<IHttpContextAccessor>();
            if (accessor is not { HttpContext: { Request: { } } }) return;

            var req = accessor.HttpContext.Request;
            var baseUrl = $"{req.Scheme}://{req.Host}";

            model.Client.BaseAddress = new Uri(baseUrl);
        }
    }
}


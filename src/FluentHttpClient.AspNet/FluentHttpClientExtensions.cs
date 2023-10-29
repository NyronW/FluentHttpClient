using Microsoft.AspNetCore.Http;

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

    public static ISendFileActions AttachFiles(this IAttachFiles attach, IFormFile[] files)
    {
        return attach.AttachFiles(files);
    }
}


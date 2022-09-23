using Microsoft.AspNetCore.Authorization;
using MinimalEndpoints;

namespace FluentHttpClient.Demo.Api.Features.Files;

[Authorize]
public class UploadFile : IEndpoint
{
    public string Pattern => "/files";

    public HttpMethod Method => HttpMethod.Post;

    public Delegate Handler => HandleAsync;

    public async Task<IResult> HandleAsync(HttpRequest request)
    {
        if (!request.HasFormContentType) return Results.BadRequest();

        var form = await request.ReadFormAsync();
        var tempfile = string.Empty;

        foreach (var file in form.Files)
        {
            if (file.Length == 0 && string.IsNullOrEmpty(file.FileName))
                continue;

            if (file.FileName.StartsWith("error")) //simulate retryable feature on client
            {
                return Results.StatusCode(StatusCodes.Status502BadGateway);
            }

            tempfile = CreateTempfilePath();
            using var stream = File.OpenWrite(tempfile);
            await file.CopyToAsync(stream);
        }

        request.HttpContext.Response.Headers.Add("x-file-name", Path.GetFileName(tempfile));

        return Results.Ok();
    }

    private string CreateTempfilePath()
    {
        var filename = $"{Guid.NewGuid()}.tmp";
        var directoryPath = Path.Combine("temp", "uploads");
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        return Path.Combine(directoryPath, filename);
    }
}

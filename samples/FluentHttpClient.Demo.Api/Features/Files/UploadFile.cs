using MinimalEndpoints;

namespace FluentHttpClient.Demo.Api.Features.Files;

public class UploadFile : IEndpoint
{
    public string Pattern => "/files";

    public HttpMethod Method => HttpMethod.Post;

    public Delegate Handler => HandleAsync;

    public async Task<IResult> HandleAsync(HttpRequest request)
    {
        if (!request.HasFormContentType) return Results.BadRequest();

        var form = await request.ReadFormAsync();

        foreach (var file in form.Files)
        {
            if (file.Length == 0 && string.IsNullOrEmpty(file.FileName))
                continue;

            var tempfile = CreateTempfilePath();
            using var stream = File.OpenWrite(tempfile);
            await file.CopyToAsync(stream);
        }

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

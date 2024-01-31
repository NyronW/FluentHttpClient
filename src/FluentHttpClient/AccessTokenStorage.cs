using System.Net.Http.Formatting;

namespace FluentHttpClient;

public class AccessTokenStorage
{
    private static readonly SemaphoreSlim AccessTokenSemaphore;
    private static AccessToken? _accessToken;

    private readonly IHttpClientFactory _httpClientFactory;

    static AccessTokenStorage()
    {
        _accessToken = null!;
        AccessTokenSemaphore = new SemaphoreSlim(1, 1);
    }

    public AccessTokenStorage(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AccessToken> GetToken(string identityUrl, string clientId, string clientSecret, string[]? scopes = null)
    {
        try
        {
            await AccessTokenSemaphore.WaitAsync();

            if (_accessToken is { Expired: false })
            {
                return _accessToken;
            }

            var client = _httpClientFactory.CreateClient();


            var request = new HttpRequestMessage(HttpMethod.Post, identityUrl)
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string?, string?>[]
                {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("scope", string.Join(" ", scopes ?? Array.Empty<string>())),
                new("grant_type", "client_credentials")
                })
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var accessToken = await response.Content.ReadAsAsync<AccessToken>(new MediaTypeFormatterCollection());

            return accessToken ?? throw new Exception("Failed to deserialize access token");
        }
        finally
        {
            AccessTokenSemaphore.Release(1);
        }
    }
}


public class AccessToken
{
    // Let token "expire" 5 minutes before it's actual expiration
    // to avoid using expired tokens and getting 401.
    private static readonly TimeSpan Threshold = new(0, 5, 0);

    public AccessToken(
        string access_token,
        int expires_in)
    {
        Token = access_token;
        ExpiresInSeconds = expires_in;
        Expires = DateTime.UtcNow.AddSeconds(ExpiresInSeconds);
    }

    public string Token { get; }
    public int ExpiresInSeconds { get; }
    public DateTime Expires { get; }
    public bool Expired => (Expires - DateTime.UtcNow).TotalSeconds <= Threshold.TotalSeconds;

    public static AccessToken Empty => new(string.Empty, 0);
}
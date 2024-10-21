using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.Json;

namespace FluentHttpClient;

public class AccessTokenStorage
{
    private static readonly SemaphoreSlim AccessTokenSemaphore = new (1, 1);
    private static AccessToken? _accessToken = null;

    private readonly IHttpClientFactory _httpClientFactory;

    public AccessTokenStorage(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AccessToken> GetToken(string identityUrl, string clientId, string clientSecret, string[]? scopes = null)
    {
        if (_accessToken is { Expired: false })
        {
            return _accessToken;
        }

        await AccessTokenSemaphore.WaitAsync();

        try
        {
            if (_accessToken is { Expired: false })
            {
                return _accessToken;
            }

            var tokenEndpoint = await GetTokenEndpoint(identityUrl);
            if (string.IsNullOrEmpty(tokenEndpoint))
            {
                throw new InvalidOperationException("Token endpoint not found in discovery response.");
            }

            _accessToken = await FetchNewToken(tokenEndpoint, clientId, clientSecret, scopes);
            return _accessToken;
        }
        finally
        {
            AccessTokenSemaphore.Release(1);
        }
    }

    private async Task<string?> GetTokenEndpoint(string identityUrl)
    {
        var client = _httpClientFactory.CreateClient();
        var discoveryEndpoint = $"{identityUrl}/.well-known/openid-configuration";

        var discoveryResponse = await client.GetStringAsync(discoveryEndpoint);
        var discoveryData = JsonDocument.Parse(discoveryResponse);

        return discoveryData.RootElement.GetProperty("token_endpoint").GetString();
    }

    private async Task<AccessToken> FetchNewToken(
     string tokenEndpoint, string clientId, string clientSecret, string[]? scopes)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", string.Join(" ", scopes ?? []))
            ])
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenData = JsonDocument.Parse(responseContent);

        var accessToken = tokenData.RootElement.GetProperty("access_token").GetString();
        var expiresIn = tokenData.RootElement.GetProperty("expires_in").GetInt32();

        return new AccessToken(accessToken ?? string.Empty, expiresIn);
    }
}


public class AccessToken
{
    // Let token "expire" 5 minutes before it's actual expiration
    // to avoid using expired tokens and getting 401.
    private static readonly TimeSpan Threshold = TimeSpan.FromMinutes(5);

    public AccessToken(string accessToken, int expiresInSeconds)
    {
        Token = accessToken;
        ExpiresInSeconds = expiresInSeconds;
        Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds).Subtract(Threshold);
    }

    public string Token { get; }
    public int ExpiresInSeconds { get; }
    public DateTime Expires { get; }
    public bool Expired => DateTime.UtcNow >= Expires;

    public static AccessToken Empty => new(string.Empty, 0);
}
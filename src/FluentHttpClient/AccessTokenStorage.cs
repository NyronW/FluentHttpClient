using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentHttpClient;

public class AccessTokenStorage(IHttpClientFactory httpClientFactory)
{
    private static readonly SemaphoreSlim AccessTokenSemaphore = new (1, 1);
    private static AccessToken? _accessToken;

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

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

            string? tokenEndpoint = await GetTokenEndpoint(identityUrl);
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
        using HttpClient client = _httpClientFactory.CreateClient();
        string discoveryEndpoint = $"{identityUrl}/.well-known/openid-configuration";

        string discoveryResponse = await client.GetStringAsync(discoveryEndpoint);
        var discoveryData = JsonDocument.Parse(discoveryResponse);

        return discoveryData.RootElement.GetProperty("token_endpoint").GetString();
    }

    private async Task<AccessToken> FetchNewToken(
     string tokenEndpoint, string clientId, string clientSecret, string[]? scopes)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", string.Join(" ", scopes ?? []))
            ])
        };

        using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        var tokenData = JsonDocument.Parse(responseContent);

        string? accessToken = tokenData.RootElement.GetProperty("access_token").GetString();
        int expiresIn = tokenData.RootElement.GetProperty("expires_in").GetInt32();

        return new AccessToken(accessToken ?? string.Empty, expiresIn);
    }
}


public class AccessToken
{
    public AccessToken(string accessToken, int expiresInSeconds)
    {
        Token = accessToken;
        ExpiresInSeconds = expiresInSeconds;
    }

    [Newtonsoft.Json.JsonProperty("access_token")]
    [JsonPropertyName("access_token")]
    public string Token { get; set; }

    [Newtonsoft.Json.JsonProperty("expires_in")]
    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; set; }

    public DateTime Expires => DateTime.UtcNow.AddSeconds(ExpiresInSeconds);

    public bool Expired => DateTime.UtcNow >= Expires;

    public static AccessToken Empty => new(string.Empty, 0);
}

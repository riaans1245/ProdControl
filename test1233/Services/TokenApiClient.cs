using System.Net.Http.Json;
using System.Text.Json;
using test1233.Models;

namespace test1233.Services;

public class TokenApiClient(IHttpClientFactory httpClientFactory) : ITokenApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<UseTokenApiResponse> SendUsedTokenAsync(
        string baseUrl,
        string? cookieHeader,
        UseTokenApiRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/tokens/use")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<UseTokenApiResponse>(JsonOptions, cancellationToken);

        return payload ?? new UseTokenApiResponse
        {
            Success = false,
            Message = response.IsSuccessStatusCode
                ? "The API returned an empty response."
                : "The API request failed."
        };
    }

    public async Task<IReadOnlyCollection<AppUsedToken>> GetUsedTokensAsync(
        string baseUrl,
        string? cookieHeader,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/tokens/used");

        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<AppUsedToken>();
        }

        var payload = await response.Content.ReadFromJsonAsync<List<AppUsedToken>>(JsonOptions, cancellationToken);
        if (payload is null)
        {
            return Array.Empty<AppUsedToken>();
        }

        return payload.AsReadOnly();
    }
}

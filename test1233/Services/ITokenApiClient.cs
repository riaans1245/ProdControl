using test1233.Models;

namespace test1233.Services;

public interface ITokenApiClient
{
    Task<UseTokenApiResponse> SendUsedTokenAsync(
        string baseUrl,
        string? cookieHeader,
        UseTokenApiRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AppUsedToken>> GetUsedTokensAsync(
        string baseUrl,
        string? cookieHeader,
        CancellationToken cancellationToken = default);
}

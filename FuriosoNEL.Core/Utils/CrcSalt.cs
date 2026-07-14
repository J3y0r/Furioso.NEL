using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FuriosoNEL.Core.Utils;

public static class CrcSalt
{
    private const string FallbackSalt = "EFD76BB364D9C8BD90767B6F51F574F3";
    private const string CrcApiUrl = "https://crcsalt.cfd/crcsalt";
    private const string CrcApiToken = "";
    private static string _cachedSalt = FallbackSalt;

    public static string GetCached() => _cachedSalt;

    public static async Task<string> RefreshAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(CrcApiToken))
            return _cachedSalt;

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, CrcApiUrl)
            {
                Content = JsonContent.Create(new { })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CrcApiToken);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return _cachedSalt;

            var result = await response.Content.ReadFromJsonAsync<CrcSaltResponse>(cancellationToken: ct);
            if (result?.Success == true && !string.IsNullOrWhiteSpace(result.Salt))
                _cachedSalt = result.Salt;
        }
        catch (HttpRequestException)
        {
        }
        catch (JsonException)
        {
        }

        return _cachedSalt;
    }

    private sealed class CrcSaltResponse
    {
        public bool Success { get; init; }
        public string? Salt { get; init; }
    }
}

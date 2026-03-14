using System.Text.Json;
using MediaTracker.Models;

namespace MediaTracker.Services;

public class RawgService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.rawg.io/api";

    public RawgService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKeys:Rawg"] ?? string.Empty;
    }

    public async Task<List<SearchResult>> SearchGamesAsync(string query)
    {
        var url = $"{BaseUrl}/games?key={_apiKey}&search={Uri.EscapeDataString(query)}&page_size=8";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);
        var results = new List<SearchResult>();

        if (!json.RootElement.TryGetProperty("results", out var items)) return results;

        foreach (var item in items.EnumerateArray())
        {
            var genres = item.TryGetProperty("genres", out var g)
                ? string.Join(", ", g.EnumerateArray().Select(x => x.GetProperty("name").GetString()))
                : null;

            results.Add(new SearchResult
            {
                ExternalId = item.GetProperty("id").GetInt32().ToString(),
                MediaType = MediaType.VideoGame,
                Title = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                CoverImageUrl = item.TryGetProperty("background_image", out var bg) ? bg.GetString() : null,
                ReleaseDate = item.TryGetProperty("released", out var r) ? r.GetString() : null,
                Rating = item.TryGetProperty("rating", out var rat) ? rat.GetDouble() : null,
                Genre = genres,
            });
        }
        return results;
    }

    public async Task<SearchResult?> GetGameDetailsAsync(string id)
    {
        var url = $"{BaseUrl}/games/{id}?key={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        var item = JsonDocument.Parse(response).RootElement;

        var genres = item.TryGetProperty("genres", out var g)
            ? string.Join(", ", g.EnumerateArray().Select(x => x.GetProperty("name").GetString()))
            : null;

        var devs = item.TryGetProperty("developers", out var d)
            ? string.Join(", ", d.EnumerateArray().Select(x => x.GetProperty("name").GetString()))
            : null;

        return new SearchResult
        {
            ExternalId = id,
            MediaType = MediaType.VideoGame,
            Title = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Description = item.TryGetProperty("description_raw", out var desc) ? desc.GetString() : null,
            CoverImageUrl = item.TryGetProperty("background_image", out var bg) ? bg.GetString() : null,
            ReleaseDate = item.TryGetProperty("released", out var r) ? r.GetString() : null,
            Rating = item.TryGetProperty("rating", out var rat) ? rat.GetDouble() : null,
            Genre = genres,
            Creator = devs,
        };
    }
}

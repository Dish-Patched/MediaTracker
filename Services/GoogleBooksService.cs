using System.Text.Json;
using MediaTracker.Models;

namespace MediaTracker.Services;

public class GoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://www.googleapis.com/books/v1";

    public GoogleBooksService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKeys:GoogleBooks"] ?? string.Empty;
    }

    public async Task<List<SearchResult>> SearchBooksAsync(string query)
    {
        var keyParam = string.IsNullOrEmpty(_apiKey) ? "" : $"&key={_apiKey}";
        var url = $"{BaseUrl}/volumes?q={Uri.EscapeDataString(query)}&maxResults=8{keyParam}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);
        var results = new List<SearchResult>();

        if (!json.RootElement.TryGetProperty("items", out var items)) return results;

        foreach (var item in items.EnumerateArray())
        {
            var info = item.GetProperty("volumeInfo");

            // fix http -> https
            string? thumbnail = null;
            if (info.TryGetProperty("imageLinks", out var imgLinks) && imgLinks.TryGetProperty("thumbnail", out var tnUrl))
                thumbnail = tnUrl.GetString()?.Replace("http://", "https://");

            var authors = info.TryGetProperty("authors", out var a)
                ? string.Join(", ", a.EnumerateArray().Select(x => x.GetString()))
                : null;

            var categories = info.TryGetProperty("categories", out var cats)
                ? string.Join(", ", cats.EnumerateArray().Select(x => x.GetString()))
                : null;

            results.Add(new SearchResult
            {
                ExternalId = item.GetProperty("id").GetString() ?? "",
                MediaType = MediaType.Book,
                Title = info.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                Description = info.TryGetProperty("description", out var d) ? d.GetString() : null,
                CoverImageUrl = thumbnail,
                ReleaseDate = info.TryGetProperty("publishedDate", out var pd) ? pd.GetString() : null,
                Creator = authors,
                Genre = categories,
                Rating = info.TryGetProperty("averageRating", out var ar) ? ar.GetDouble() : null,
            });
        }
        return results;
    }
}

using System.Text.Json;
using MediaTracker.Models;

namespace MediaTracker.Services;

public class TmdbService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.themoviedb.org/3";
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";

    public TmdbService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKeys:Tmdb"] ?? string.Empty;
    }

    public async Task<List<SearchResult>> SearchMoviesAsync(string query)
    {
        var url = $"{BaseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);
        var results = new List<SearchResult>();

        foreach (var item in json.RootElement.GetProperty("results").EnumerateArray().Take(8))
        {
            results.Add(new SearchResult
            {
                ExternalId = item.GetProperty("id").GetInt32().ToString(),
                MediaType = MediaType.Movie,
                Title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                Description = item.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                CoverImageUrl = item.TryGetProperty("poster_path", out var pp) && pp.GetString() != null
                    ? $"{ImageBaseUrl}{pp.GetString()}" : null,
                ReleaseDate = item.TryGetProperty("release_date", out var rd) ? rd.GetString() : null,
                Rating = item.TryGetProperty("vote_average", out var va) ? va.GetDouble() : null,
            });
        }
        return results;
    }

    public async Task<List<SearchResult>> SearchTvShowsAsync(string query)
    {
        var url = $"{BaseUrl}/search/tv?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);
        var results = new List<SearchResult>();

        foreach (var item in json.RootElement.GetProperty("results").EnumerateArray().Take(8))
        {
            results.Add(new SearchResult
            {
                ExternalId = item.GetProperty("id").GetInt32().ToString(),
                MediaType = MediaType.TvShow,
                Title = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                Description = item.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                CoverImageUrl = item.TryGetProperty("poster_path", out var pp) && pp.GetString() != null
                    ? $"{ImageBaseUrl}{pp.GetString()}" : null,
                ReleaseDate = item.TryGetProperty("first_air_date", out var fad) ? fad.GetString() : null,
                Rating = item.TryGetProperty("vote_average", out var va) ? va.GetDouble() : null,
            });
        }
        return results;
    }

    public async Task<SearchResult?> GetMovieDetailsAsync(string id)
    {
        var url = $"{BaseUrl}/movie/{id}?api_key={_apiKey}&append_to_response=credits";
        var response = await _httpClient.GetStringAsync(url);
        var item = JsonDocument.Parse(response).RootElement;

        string? director = null;
        if (item.TryGetProperty("credits", out var credits) && credits.TryGetProperty("crew", out var crew))
        {
            director = crew.EnumerateArray()
                .FirstOrDefault(c => c.TryGetProperty("job", out var job) && job.GetString() == "Director")
                .TryGetProperty("name", out var dn) ? dn.GetString() : null;
        }

        var genres = item.TryGetProperty("genres", out var g)
            ? string.Join(", ", g.EnumerateArray().Select(x => x.GetProperty("name").GetString()))
            : null;

        return new SearchResult
        {
            ExternalId = id,
            MediaType = MediaType.Movie,
            Title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
            Description = item.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
            CoverImageUrl = item.TryGetProperty("poster_path", out var pp) && pp.GetString() != null
                ? $"{ImageBaseUrl}{pp.GetString()}" : null,
            ReleaseDate = item.TryGetProperty("release_date", out var rd) ? rd.GetString() : null,
            Rating = item.TryGetProperty("vote_average", out var va) ? va.GetDouble() : null,
            Genre = genres,
            Creator = director,
        };
    }

    public async Task<SearchResult?> GetTvShowDetailsAsync(string id)
    {
        var url = $"{BaseUrl}/tv/{id}?api_key={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        var item = JsonDocument.Parse(response).RootElement;

        string? creator = null;
        if (item.TryGetProperty("created_by", out var cb) && cb.GetArrayLength() > 0)
            creator = cb[0].TryGetProperty("name", out var cn) ? cn.GetString() : null;

        var genres = item.TryGetProperty("genres", out var g)
            ? string.Join(", ", g.EnumerateArray().Select(x => x.GetProperty("name").GetString()))
            : null;

        return new SearchResult
        {
            ExternalId = id,
            MediaType = MediaType.TvShow,
            Title = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Description = item.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
            CoverImageUrl = item.TryGetProperty("poster_path", out var pp) && pp.GetString() != null
                ? $"{ImageBaseUrl}{pp.GetString()}" : null,
            ReleaseDate = item.TryGetProperty("first_air_date", out var fad) ? fad.GetString() : null,
            Rating = item.TryGetProperty("vote_average", out var va) ? va.GetDouble() : null,
            Genre = genres,
            Creator = creator,
        };
    }
}

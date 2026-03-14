using MediaTracker.Models;

namespace MediaTracker.Services;

public interface IMediaSearchService
{
    Task<List<SearchResult>> SearchAsync(string query, MediaType mediaType);
}

namespace MediaTracker.Models;

public class SearchResult
{
    public string ExternalId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Creator { get; set; }
    public double? Rating { get; set; }
}

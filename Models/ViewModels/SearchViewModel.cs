namespace MediaTracker.Models.ViewModels;

public class SearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public List<SearchResult> Results { get; set; } = new();
}

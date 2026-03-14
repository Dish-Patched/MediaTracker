namespace MediaTracker.Models.ViewModels;

public class DashboardViewModel
{
    public List<MediaItem> AllItems { get; set; } = new();
    public List<MediaItem> Movies { get; set; } = new();
    public List<MediaItem> TvShows { get; set; } = new();
    public List<MediaItem> Books { get; set; } = new();
    public List<MediaItem> VideoGames { get; set; } = new();
}

namespace MediaTracker.Models;

public enum MediaType
{
    Movie,
    TvShow,
    Book,
    VideoGame
}

public enum WatchStatus
{
    PlanTo,
    InProgress,
    Completed,
    Dropped
}

public class MediaItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string ExternalId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Creator { get; set; } // director, author, developer
    public double? Rating { get; set; } // external rating

    public WatchStatus Status { get; set; } = WatchStatus.PlanTo;
    public int? UserRating { get; set; } // 1-10
    public string? UserNotes { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}

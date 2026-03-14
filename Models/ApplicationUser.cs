using Microsoft.AspNetCore.Identity;

namespace MediaTracker.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
}

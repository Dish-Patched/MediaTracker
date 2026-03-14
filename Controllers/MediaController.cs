using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediaTracker.Data;
using MediaTracker.Models;
using MediaTracker.Models.ViewModels;
using MediaTracker.Services;

namespace MediaTracker.Controllers;

[Authorize]
public class MediaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TmdbService _tmdb;
    private readonly GoogleBooksService _books;
    private readonly RawgService _rawg;

    public MediaController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        TmdbService tmdb,
        GoogleBooksService books,
        RawgService rawg)
    {
        _context = context;
        _userManager = userManager;
        _tmdb = tmdb;
        _books = books;
        _rawg = rawg;
    }

    [HttpGet]
    public IActionResult Search() => View(new SearchViewModel());

    [HttpPost]
    public async Task<IActionResult> Search(SearchViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Query))
            return View(model);

        model.Results = model.MediaType switch
        {
            MediaType.Movie => await _tmdb.SearchMoviesAsync(model.Query),
            MediaType.TvShow => await _tmdb.SearchTvShowsAsync(model.Query),
            MediaType.Book => await _books.SearchBooksAsync(model.Query),
            MediaType.VideoGame => await _rawg.SearchGamesAsync(model.Query),
            _ => new List<SearchResult>()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddMedia(string externalId, MediaType mediaType)
    {
        var userId = _userManager.GetUserId(User)!;

        // Check if already added
        var existing = await _context.MediaItems
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ExternalId == externalId && m.MediaType == mediaType);

        if (existing != null)
        {
            TempData["Error"] = "This item is already in your collection.";
            return RedirectToAction("Index", "Dashboard");
        }

        // Fetch full details
        SearchResult? details = mediaType switch
        {
            MediaType.Movie => await _tmdb.GetMovieDetailsAsync(externalId),
            MediaType.TvShow => await _tmdb.GetTvShowDetailsAsync(externalId),
            MediaType.VideoGame => await _rawg.GetGameDetailsAsync(externalId),
            _ => null
        };

        // For books, use the Google Books volumes API by ID
        if (mediaType == MediaType.Book)
        {
            details = await GetBookByIdAsync(externalId);
        }

        if (details == null)
        {
            TempData["Error"] = "Could not fetch media details.";
            return RedirectToAction("Index", "Dashboard");
        }

        var item = new MediaItem
        {
            UserId = userId,
            ExternalId = details.ExternalId,
            MediaType = details.MediaType,
            Title = details.Title,
            Description = details.Description,
            CoverImageUrl = details.CoverImageUrl,
            ReleaseDate = details.ReleaseDate,
            Genre = details.Genre,
            Creator = details.Creator,
            Rating = details.Rating,
            Status = WatchStatus.PlanTo,
        };

        _context.MediaItems.Add(item);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"\"{item.Title}\" added to your collection!";
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task<SearchResult?> GetBookByIdAsync(string id)
    {
        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"https://www.googleapis.com/books/v1/volumes/{id}");
            var json = System.Text.Json.JsonDocument.Parse(response).RootElement;
            var info = json.GetProperty("volumeInfo");

            string? thumbnail = null;
            if (info.TryGetProperty("imageLinks", out var imgLinks) && imgLinks.TryGetProperty("thumbnail", out var tnUrl))
                thumbnail = tnUrl.GetString()?.Replace("http://", "https://");

            var authors = info.TryGetProperty("authors", out var a)
                ? string.Join(", ", a.EnumerateArray().Select(x => x.GetString()))
                : null;

            var categories = info.TryGetProperty("categories", out var cats)
                ? string.Join(", ", cats.EnumerateArray().Select(x => x.GetString()))
                : null;

            return new SearchResult
            {
                ExternalId = id,
                MediaType = MediaType.Book,
                Title = info.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                Description = info.TryGetProperty("description", out var d) ? d.GetString() : null,
                CoverImageUrl = thumbnail,
                ReleaseDate = info.TryGetProperty("publishedDate", out var pd) ? pd.GetString() : null,
                Creator = authors,
                Genre = categories,
                Rating = info.TryGetProperty("averageRating", out var ar) ? ar.GetDouble() : null,
            };
        }
        catch { return null; }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _context.MediaItems.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(MediaItem model)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _context.MediaItems.FirstOrDefaultAsync(m => m.Id == model.Id && m.UserId == userId);
        if (item == null) return NotFound();

        item.Status = model.Status;
        item.UserRating = model.UserRating;
        item.UserNotes = model.UserNotes;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Updated successfully.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _context.MediaItems.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (item != null)
        {
            _context.MediaItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"\"{item.Title}\" removed from your collection.";
        }
        return RedirectToAction("Index", "Dashboard");
    }
}

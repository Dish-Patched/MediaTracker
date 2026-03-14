using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediaTracker.Data;
using MediaTracker.Models;
using MediaTracker.Models.ViewModels;

namespace MediaTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var items = await _context.MediaItems
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.DateAdded)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            AllItems = items,
            Movies = items.Where(m => m.MediaType == MediaType.Movie).ToList(),
            TvShows = items.Where(m => m.MediaType == MediaType.TvShow).ToList(),
            Books = items.Where(m => m.MediaType == MediaType.Book).ToList(),
            VideoGames = items.Where(m => m.MediaType == MediaType.VideoGame).ToList(),
        };

        return View(vm);
    }
}

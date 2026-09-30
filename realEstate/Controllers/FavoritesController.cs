using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realEstate.Models;
using realEstate.Repositories;
using Microsoft.AspNetCore.Identity;

namespace realEstate.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly IFavoriteRepository _favorites;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(IFavoriteRepository favorites, UserManager<ApplicationUser> userManager)
    {
        _favorites = favorites;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var properties = await _favorites.GetFavoritesForUserAsync(user.Id);
        return View(properties);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int propertyId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        await _favorites.AddFavoriteAsync(user.Id, propertyId);
        return RedirectToAction("Index", "Property");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int propertyId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        await _favorites.RemoveFavoriteAsync(user.Id, propertyId);
        return RedirectToAction("Index");
    }
}

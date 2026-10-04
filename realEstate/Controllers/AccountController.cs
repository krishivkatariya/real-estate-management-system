using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using realEstate.Models;
using realEstate.Models.AccountViewModels;

namespace realEstate.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = false };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Generate email confirmation token and log the confirmation link (development)
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("VerifyEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
            _logger.LogInformation("Verification Link: {Link}", confirmationLink);

            // Assign role based on registration input (Buyer, Seller or Admin)
            try
            {
                var desiredRole = "Buyer";
                if (Request.Form.TryGetValue("Role", out var rv))
                {
                    var r = rv.ToString();
                    if (r == "Seller" || r == "Buyer" || r == "Admin") desiredRole = r;
                }

                // Ensure role exists before assigning
                if (!await _roleManager.RoleExistsAsync(desiredRole))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(desiredRole));
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to create role {Role}: {Errors}", desiredRole, string.Join(';', roleResult.Errors.Select(e => e.Description)));
                    }
                }

                var addResult = await _userManager.AddToRoleAsync(user, desiredRole);
                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add user {User} to role {Role}: {Errors}", user.Email, desiredRole, string.Join(';', addResult.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role during registration");
            }

            // Sign in the newly registered user and redirect to role-specific dashboard
            await _signInManager.SignInAsync(user, isPersistent: false);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Admin", "Dashboard");
            if (await _userManager.IsInRoleAsync(user, "Seller"))
                return RedirectToAction("Seller", "Dashboard");

            return RedirectToAction("Buyer", "Dashboard");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Your email has been confirmed. Please log in.";
            return RedirectToAction("Login");
        }

        return View("VerifyEmailError");
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (User?.Identity?.IsAuthenticated ?? false)
        {
            var current = await _userManager.GetUserAsync(User);
            if (current != null)
            {
                if (await _userManager.IsInRoleAsync(current, "Admin"))
                    return RedirectToAction("Admin", "Dashboard");
                if (await _userManager.IsInRoleAsync(current, "Seller"))
                    return RedirectToAction("Seller", "Dashboard");
                return RedirectToAction("Buyer", "Dashboard");
            }
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // If a returnUrl was provided, prefer it
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Otherwise redirect based on role priority: Admin > Seller > Buyer
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Admin", "Dashboard");
                if (await _userManager.IsInRoleAsync(user, "Seller"))
                    return RedirectToAction("Seller", "Dashboard");
                return RedirectToAction("Buyer", "Dashboard");
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

using ByteMe.Data;
using ByteMe.Helpers;
using ByteMe.Models;
using ByteMe.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class AccountController : BaseController
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.PasswordHash))
        {
            ViewBag.Error = "All fields are required.";
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ViewBag.Error = "Email already registered.";
            return View(model);
        }

        var user = new User
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHasher.Hash(model.PasswordHash),
            Role = "Customer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? role)
    {
        if (IsLoggedIn())
        {
            SessionManager.SyncFromSession(HttpContext.Session);
            return SessionManager.UserRole == "Admin"
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Index", "Home");
        }

        ViewBag.SelectedRole = role == "Admin" ? "Admin" : "Customer";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string role)
    {
        ViewBag.SelectedRole = role == "Admin" ? "Admin" : "Customer";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Email and password are required.";
            return View();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant());

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        var expectedRole = role == "Admin" ? "Admin" : "Customer";
        if (user.Role != expectedRole)
        {
            ViewBag.Error = expectedRole == "Admin"
                ? "This account is not an administrator. Try Customer Login."
                : "This account is an administrator. Use Admin Login.";
            return View();
        }

        SetSessionUser(user.Id, user.Name, user.Role);

        if (user.Role == "Admin")
            return RedirectToAction("Dashboard", "Admin");

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Profile()
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.FoodItem)
            .ThenInclude(f => f!.Category)
            .Where(o => o.UserId == userId.Value)
            .ToListAsync();

        string favoriteCategory = "N/A";
        var categoryCounts = orders
            .SelectMany(o => o.OrderItems)
            .Where(oi => oi.FoodItem?.Category != null)
            .GroupBy(oi => oi.FoodItem!.Category!.Name)
            .Select(g => new { Name = g.Key, Count = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        if (categoryCounts != null)
            favoriteCategory = categoryCounts.Name;

        var model = new ProfileViewModel
        {
            User = user,
            TotalOrders = orders.Count,
            TotalSpent = orders.Sum(o => o.TotalAmount),
            FavoriteCategory = favoriteCategory,
            RecentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList()
        };

        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string name)
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");

        if (!string.IsNullOrWhiteSpace(name))
        {
            user.Name = name.Trim();
            await _context.SaveChangesAsync();
            SetSessionUser(user.Id, user.Name, user.Role);
            TempData["Success"] = "Profile updated successfully.";
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Logout()
    {
        ClearSessionUser();
        return RedirectToAction("Login", "Account");
    }
}

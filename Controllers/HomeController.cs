using ByteMe.Data;
using ByteMe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class HomeController : BaseController
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        var allItems = await _context.FoodItems.Where(f => f.IsAvailable).ToListAsync();
        var featured = allItems.OrderBy(_ => Guid.NewGuid()).Take(4).ToList();

        ViewBag.FeaturedItems = featured;
        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubscribeNewsletter(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            TempData["NewsletterError"] = "Please enter a valid email address.";
            return RedirectToAction(nameof(Index));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (!await _context.NewsletterSubscribers.AnyAsync(n => n.Email == normalized))
        {
            _context.NewsletterSubscribers.Add(new NewsletterSubscriber { Email = normalized });
            await _context.SaveChangesAsync();
        }

        TempData["NewsletterSuccess"] = "You're subscribed! Watch your inbox for deals.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}

using ByteMe.Data;
using ByteMe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class FeedbackController : BaseController
{
    private readonly AppDbContext _context;

    public FeedbackController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        ViewBag.FoodItems = await _context.FoodItems
            .Where(f => f.IsAvailable)
            .OrderBy(f => f.Name)
            .Select(f => f.Name)
            .ToListAsync();

        var myFeedbacks = await _context.Feedbacks
            .Where(f => f.UserId == userId.Value)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();

        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View(myFeedbacks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(string foodItemName, int rating, string comment)
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(foodItemName) || rating < 1 || rating > 5)
        {
            TempData["Error"] = "Please select a food item and rating (1-5 stars).";
            return RedirectToAction(nameof(Index));
        }

        _context.Feedbacks.Add(new Feedback
        {
            UserId = userId.Value,
            FoodItemName = foodItemName.Trim(),
            Rating = rating,
            Comment = comment?.Trim() ?? string.Empty,
            IsApproved = false
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thank you! Your feedback has been submitted for review.";
        return RedirectToAction(nameof(Index));
    }
}

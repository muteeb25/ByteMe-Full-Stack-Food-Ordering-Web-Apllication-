using ByteMe.Data;
using ByteMe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class MenuController : BaseController
{
    private readonly AppDbContext _context;

    public MenuController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search)
    {
        var categories = await _context.Categories.ToListAsync();
        var query = _context.FoodItems
            .Include(f => f.Category)
            .Where(f => f.IsAvailable);

        if (categoryId.HasValue)
            query = query.Where(f => f.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(f =>
                f.Name.ToLower().Contains(term) ||
                f.Description.ToLower().Contains(term) ||
                (f.Category != null && f.Category.Name.ToLower().Contains(term)));
        }

        var items = await query.ToListAsync();
        var approvedFeedbacks = await _context.Feedbacks
            .Include(f => f.User)
            .Where(f => f.IsApproved)
            .OrderByDescending(f => f.SubmittedAt)
            .Take(12)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.Search = search;
        ViewBag.ApprovedFeedbacks = approvedFeedbacks;
        ViewBag.CartCount = await GetCartCountAsync(_context);

        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.FoodItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (item == null)
            return NotFound();

        var reviews = await _context.Feedbacks
            .Include(f => f.User)
            .Where(f => f.IsApproved && f.FoodItemName == item.Name)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();

        var related = await _context.FoodItems
            .Include(f => f.Category)
            .Where(f => f.CategoryId == item.CategoryId && f.Id != item.Id && f.IsAvailable)
            .Take(3)
            .ToListAsync();

        ViewBag.Reviews = reviews;
        ViewBag.RelatedItems = related;
        ViewBag.CartCount = await GetCartCountAsync(_context);

        return View(item);
    }
}

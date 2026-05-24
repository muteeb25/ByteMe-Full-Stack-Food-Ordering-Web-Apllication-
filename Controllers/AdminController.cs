using ByteMe.Data;
using ByteMe.Models;
using ByteMe.Models.ViewModels;
using ByteMe.Services.Patterns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class AdminController : BaseController
{
    private readonly AppDbContext _context;
    private readonly OrderNotificationService _notificationService;

    public AdminController(AppDbContext context, OrderNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    private IActionResult? RequireAdmin()
    {
        SessionManager.SyncFromSession(HttpContext.Session);
        if (!IsLoggedIn() || SessionManager.UserRole != "Admin")
            return RedirectToAction("Login", "Account", new { role = "Admin" });
        return null;
    }

    private void SetAdminViewBag()
    {
        SessionManager.SyncFromSession(HttpContext.Session);
        ViewBag.AdminName = SessionManager.UserName;
        ViewBag.ActiveNav = ViewData["ActiveNav"]?.ToString() ?? "";
    }

    public async Task<IActionResult> Dashboard()
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Dashboard";
        SetAdminViewBag();

        var orders = await _context.Orders.ToListAsync();
        var topFoodRaw = await _context.OrderItems
            .GroupBy(oi => oi.FoodItemId)
            .Select(g => new { FoodItemId = g.Key, Count = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var foodIds = topFoodRaw.Select(x => x.FoodItemId).ToList();
        var foods = await _context.FoodItems.Where(f => foodIds.Contains(f.Id)).ToListAsync();

        var model = new AdminDashboardViewModel
        {
            TotalFoodItems = await _context.FoodItems.CountAsync(),
            ActiveOrders = orders.Count(o => o.Status is "Placed" or "Preparing"),
            RegisteredUsers = await _context.Users.CountAsync(u => u.Role == "Customer"),
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            TotalFeedback = await _context.Feedbacks.CountAsync(),
            NewsletterSubscribers = await _context.NewsletterSubscribers.CountAsync(),
            RecentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Take(8)
                .ToListAsync(),
            TopFoodItems = topFoodRaw.Select(t =>
            {
                var f = foods.FirstOrDefault(x => x.Id == t.FoodItemId);
                return new TopFoodItemViewModel
                {
                    FoodItemId = t.FoodItemId,
                    Name = f?.Name ?? "Unknown",
                    ImageUrl = f?.ImageUrl ?? "",
                    OrderCount = t.Count,
                    Price = f?.Price ?? 0
                };
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> FoodItems()
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "FoodItems";
        SetAdminViewBag();

        ViewBag.Categories = await _context.Categories.ToListAsync();
        var items = await _context.FoodItems.Include(f => f.Category).OrderBy(f => f.Name).ToListAsync();
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFoodItem(FoodItem model)
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;

        if (string.IsNullOrWhiteSpace(model.Name) || model.Price <= 0)
        {
            TempData["Error"] = "Name and valid price are required.";
            return RedirectToAction(nameof(FoodItems));
        }

        _context.FoodItems.Add(new FoodItem
        {
            Name = model.Name.Trim(),
            Description = model.Description ?? string.Empty,
            Price = model.Price,
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "/images/food/default.svg" : model.ImageUrl,
            CategoryId = model.CategoryId,
            IsAvailable = Request.Form.ContainsKey("IsAvailable")
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = "Food item added.";
        return RedirectToAction(nameof(FoodItems));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFoodItemAjax(FoodItem model)
    {
        if (RequireAdmin() is IActionResult redirect) return Json(new { success = false });

        var item = await _context.FoodItems.FindAsync(model.Id);
        if (item == null) return Json(new { success = false });

        item.Name = model.Name.Trim();
        item.Description = model.Description ?? string.Empty;
        item.Price = model.Price;
        item.ImageUrl = model.ImageUrl ?? "/images/food/default.svg";
        item.CategoryId = model.CategoryId;
        item.IsAvailable = Request.Form.ContainsKey("IsAvailable") || model.IsAvailable;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetFoodItem(int id)
    {
        if (RequireAdmin() is IActionResult redirect) return Unauthorized();

        var item = await _context.FoodItems.Include(f => f.Category).FirstOrDefaultAsync(f => f.Id == id);
        if (item == null) return NotFound();

        return Json(new
        {
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.ImageUrl,
            item.CategoryId,
            item.IsAvailable
        });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFoodAvailability(int id)
    {
        if (RequireAdmin() is IActionResult redirect) return Json(new { success = false });

        var item = await _context.FoodItems.FindAsync(id);
        if (item == null) return Json(new { success = false });

        item.IsAvailable = !item.IsAvailable;
        await _context.SaveChangesAsync();

        return Json(new { success = true, isAvailable = item.IsAvailable });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFoodItem(int id)
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;

        var item = await _context.FoodItems.FindAsync(id);
        if (item != null)
        {
            _context.FoodItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(FoodItems));
    }

    public async Task<IActionResult> Orders(string? status)
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Orders";
        SetAdminViewBag();

        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.FoodItem)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && status != "All")
            query = query.Where(o => o.Status == status);

        ViewBag.CurrentStatus = status ?? "All";
        return View(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
    {
        if (RequireAdmin() is IActionResult redirect) return Json(new { success = false });

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return Json(new { success = false });

        order.Status = status;
        await _context.SaveChangesAsync();
        _notificationService.NotifyAll(order.Id, status, order.UserId);

        return Json(new { success = true });
    }

    public async Task<IActionResult> Customers()
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Customers";
        SetAdminViewBag();

        var users = await _context.Users
            .Where(u => u.Role == "Customer")
            .Include(u => u.Orders)
            .ToListAsync();

        var model = users.Select(u => new CustomerListViewModel
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            RegisteredDate = u.CreatedAt,
            TotalOrders = u.Orders.Count,
            TotalSpent = u.Orders.Sum(o => o.TotalAmount)
        }).OrderByDescending(c => c.RegisteredDate).ToList();

        return View(model);
    }

    public async Task<IActionResult> Payments()
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Payments";
        SetAdminViewBag();

        var orders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var model = new PaymentSummaryViewModel
        {
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            CashOrders = orders.Count(o => o.PaymentMethod.Contains("Cash", StringComparison.OrdinalIgnoreCase)),
            CardOrders = orders.Count(o => o.PaymentMethod.Contains("Credit", StringComparison.OrdinalIgnoreCase)),
            EasyPaisaOrders = orders.Count(o => o.PaymentMethod.Contains("EasyPaisa", StringComparison.OrdinalIgnoreCase)),
            NewsletterSubscribers = await _context.NewsletterSubscribers.CountAsync(),
            Orders = orders
        };

        return View(model);
    }

    public async Task<IActionResult> Feedbacks(string? filter)
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Feedbacks";
        SetAdminViewBag();

        var query = _context.Feedbacks.Include(f => f.User).AsQueryable();
        ViewBag.Filter = filter ?? "All";

        if (filter == "Approved")
            query = query.Where(f => f.IsApproved);
        else if (filter == "Pending")
            query = query.Where(f => !f.IsApproved);

        return View(await query.OrderByDescending(f => f.SubmittedAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> ApproveFeedback(int id)
    {
        if (RequireAdmin() is IActionResult redirect) return Json(new { success = false });

        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null) return Json(new { success = false });

        feedback.IsApproved = true;
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFeedback(int id)
    {
        if (RequireAdmin() is IActionResult redirect) return Json(new { success = false });

        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback != null)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true });
    }

    public async Task<IActionResult> Deliveries()
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Deliveries";
        SetAdminViewBag();

        var orders = await _context.Orders
            .Include(o => o.User)
            .Where(o => o.Status != "Delivered")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Messages()
    {
        if (RequireAdmin() is IActionResult redirect) return redirect;
        ViewData["ActiveNav"] = "Messages";
        SetAdminViewBag();

        var messages = await _context.ContactMessages
            .OrderByDescending(m => m.SubmittedAt)
            .ToListAsync();

        return View(messages);
    }
}

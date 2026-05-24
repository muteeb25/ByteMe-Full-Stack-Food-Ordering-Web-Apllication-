using ByteMe.Data;
using ByteMe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class CartController : BaseController
{
    private readonly AppDbContext _context;
    private const decimal DeliveryFee = 150m;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int foodItemId, int quantity = 1)
    {
        var userId = GetUserId();
        if (userId == null)
            return Json(new { success = false, message = "Please log in.", cartCount = 0 });

        if (quantity < 1) quantity = 1;

        var foodItem = await _context.FoodItems.FindAsync(foodItemId);
        if (foodItem == null || !foodItem.IsAvailable)
            return Json(new { success = false, message = "Item not available.", cartCount = await GetCartCountAsync(_context) });

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.FoodItemId == foodItemId);

        if (existing != null)
            existing.Quantity += quantity;
        else
            _context.CartItems.Add(new CartItem { UserId = userId.Value, FoodItemId = foodItemId, Quantity = quantity });

        await _context.SaveChangesAsync();
        var cartCount = await GetCartCountAsync(_context);
        return Json(new { success = true, cartCount });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Json(new { success = false, cartCount = 0 });

        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId.Value);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, cartCount = await GetCartCountAsync(_context) });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var userId = GetUserId();
        if (userId == null)
            return Json(new { success = false, newTotal = 0m });

        var item = await _context.CartItems
            .Include(c => c.FoodItem)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId.Value);

        if (item == null)
            return Json(new { success = false, newTotal = 0m });

        if (quantity < 1)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, newTotal = 0m, removed = true });
        }

        item.Quantity = quantity;
        await _context.SaveChangesAsync();

        var newTotal = item.FoodItem.Price * item.Quantity;
        return Json(new { success = true, newTotal });
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var items = await _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == userId.Value)
            .ToListAsync();

        ViewBag.Subtotal = items.Sum(i => i.FoodItem.Price * i.Quantity);
        ViewBag.DeliveryFee = DeliveryFee;
        ViewBag.Total = (decimal)ViewBag.Subtotal + DeliveryFee;
        ViewBag.CartCount = await GetCartCountAsync(_context);

        return View(items);
    }
}

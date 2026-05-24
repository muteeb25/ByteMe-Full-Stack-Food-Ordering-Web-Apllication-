using ByteMe.Data;
using ByteMe.Models;
using ByteMe.Services;
using ByteMe.Services.Interfaces;
using ByteMe.Services.Patterns;
using ByteMe.Services.Patterns.PaymentStrategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public class OrderController : BaseController
{
    private readonly AppDbContext _context;
    private readonly PaymentContext _paymentContext;
    private readonly OrderNotificationService _notificationService;
    private const decimal DeliveryFee = 150m;

    public OrderController(
        AppDbContext context,
        PaymentContext paymentContext,
        OrderNotificationService notificationService)
    {
        _context = context;
        _paymentContext = paymentContext;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Checkout()
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var cartItems = await _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == userId.Value)
            .ToListAsync();

        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        ViewBag.Subtotal = cartItems.Sum(i => i.FoodItem.Price * i.Quantity);
        ViewBag.DeliveryFee = DeliveryFee;
        ViewBag.Total = (decimal)ViewBag.Subtotal + DeliveryFee;
        ViewBag.CartCount = await GetCartCountAsync(_context);

        return View(cartItems);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(string address, string paymentMethod)
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(address))
        {
            TempData["Error"] = "Delivery address is required.";
            return RedirectToAction(nameof(Checkout));
        }

        var cartItems = await _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == userId.Value)
            .ToListAsync();

        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var subtotal = cartItems.Sum(i => i.FoodItem.Price * i.Quantity);
        var total = subtotal + DeliveryFee;

        IPaymentStrategy strategy = paymentMethod switch
        {
            "Credit Card" => new CreditCardStrategy(),
            _ => new CashOnDeliveryStrategy()
        };

        _paymentContext.SetStrategy(strategy);
        if (!_paymentContext.Execute(total))
        {
            TempData["Error"] = "Payment failed. Please try again.";
            return RedirectToAction(nameof(Checkout));
        }

        var order = new Order
        {
            UserId = userId.Value,
            TotalAmount = total,
            Status = "Placed",
            PaymentMethod = _paymentContext.GetMethodName() ?? paymentMethod,
            DeliveryAddress = address.Trim()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var cart in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                FoodItemId = cart.FoodItemId,
                Quantity = cart.Quantity,
                UnitPrice = cart.FoodItem.Price
            });
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        _notificationService.NotifyAll(order.Id, order.Status, order.UserId);

        return RedirectToAction(nameof(Confirmation), new { orderId = order.Id });
    }

    public async Task<IActionResult> Confirmation(int orderId)
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.FoodItem)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId.Value);

        if (order == null)
            return NotFound();

        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View(order);
    }

    public async Task<IActionResult> History()
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId.Value)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View(orders);
    }

    public async Task<IActionResult> Track(int? orderId)
    {
        var userId = GetUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        Order? order;
        if (orderId.HasValue)
        {
            order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId.Value && o.UserId == userId.Value);
        }
        else
        {
            order = await _context.Orders
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (order == null)
        {
            ViewBag.Message = "No orders found. Place an order first!";
            ViewBag.CartCount = await GetCartCountAsync(_context);
            return View();
        }

        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderStatus(int orderId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Json(new { success = false });

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId.Value);

        if (order == null)
            return Json(new { success = false });

        return Json(new { success = true, status = order.Status, orderId = order.Id });
    }
}

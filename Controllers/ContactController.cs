using ByteMe.Data;
using ByteMe.Models;
using Microsoft.AspNetCore.Mvc;

namespace ByteMe.Controllers;

public class ContactController : BaseController
{
    private readonly AppDbContext _context;

    public ContactController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.CartCount = await GetCartCountAsync(_context);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ContactMessage model)
    {
        ViewBag.CartCount = await GetCartCountAsync(_context);

        if (string.IsNullOrWhiteSpace(model.Name) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Message))
        {
            ViewBag.Error = "All fields are required.";
            return View("Index", model);
        }

        model.SubmittedAt = DateTime.Now;
        _context.ContactMessages.Add(model);
        await _context.SaveChangesAsync();

        ViewBag.Success = "Thank you! Your message has been sent.";
        return View("Index", new ContactMessage());
    }
}

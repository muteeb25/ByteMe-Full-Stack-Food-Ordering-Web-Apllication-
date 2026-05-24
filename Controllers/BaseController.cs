using ByteMe.Data;
using ByteMe.Services.Patterns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Controllers;

public abstract class BaseController : Controller
{
    protected UserSessionManager SessionManager => UserSessionManager.GetInstance();

    protected int? GetUserId()
    {
        SessionManager.SyncFromSession(HttpContext.Session);
        return SessionManager.IsLoggedIn ? SessionManager.UserId : null;
    }

    protected bool IsLoggedIn()
    {
        SessionManager.SyncFromSession(HttpContext.Session);
        return SessionManager.IsLoggedIn;
    }

    protected void SetSessionUser(int id, string name, string role)
    {
        SessionManager.SetUser(id, name, role);
        SessionManager.SyncToSession(HttpContext.Session);
    }

    protected void ClearSessionUser()
    {
        SessionManager.ClearUser();
        SessionManager.SyncToSession(HttpContext.Session);
        HttpContext.Session.Clear();
    }

    protected async Task<int> GetCartCountAsync(AppDbContext context)
    {
        var userId = GetUserId();
        if (userId == null) return 0;
        return await context.CartItems
            .Where(c => c.UserId == userId.Value)
            .SumAsync(c => c.Quantity);
    }
}

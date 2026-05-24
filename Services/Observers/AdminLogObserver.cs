// PATTERN: OBSERVER (concrete)
using ByteMe.Services.Interfaces;

namespace ByteMe.Services.Observers;

public class AdminLogObserver : IOrderObserver
{
    public static List<string> AdminLog { get; } = new();

    public void OnOrderStatusChanged(int orderId, string newStatus, int userId)
    {
        AdminLog.Add($"Order #{orderId} status changed to {newStatus} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}

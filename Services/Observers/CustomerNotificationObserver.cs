// PATTERN: OBSERVER (concrete)
using ByteMe.Services.Interfaces;

namespace ByteMe.Services.Observers;

public class CustomerNotificationObserver : IOrderObserver
{
    public List<string> Notifications { get; } = new();

    public void OnOrderStatusChanged(int orderId, string newStatus, int userId)
    {
        Notifications.Add($"Your order #{orderId} is now: {newStatus}!");
    }
}

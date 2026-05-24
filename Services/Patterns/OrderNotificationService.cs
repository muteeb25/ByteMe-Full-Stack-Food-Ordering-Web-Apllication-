// PATTERN: OBSERVER (subject)
using ByteMe.Services.Interfaces;

namespace ByteMe.Services.Patterns;

public class OrderNotificationService
{
    private readonly List<IOrderObserver> _observers = new();

    public void Subscribe(IOrderObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IOrderObserver observer)
    {
        _observers.Remove(observer);
    }

    public void NotifyAll(int orderId, string status, int userId)
    {
        foreach (var observer in _observers)
        {
            observer.OnOrderStatusChanged(orderId, status, userId);
        }
    }
}

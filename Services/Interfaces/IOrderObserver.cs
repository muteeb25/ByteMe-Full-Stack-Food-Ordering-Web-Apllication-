// PATTERN: OBSERVER (interface)
namespace ByteMe.Services.Interfaces;

public interface IOrderObserver
{
    void OnOrderStatusChanged(int orderId, string newStatus, int userId);
}

// PATTERN: STRATEGY (interface)
namespace ByteMe.Services.Interfaces;

public interface IPaymentStrategy
{
    string MethodName { get; }
    bool ProcessPayment(decimal amount);
}

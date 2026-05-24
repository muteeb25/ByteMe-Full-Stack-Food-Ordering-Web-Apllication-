// PATTERN: STRATEGY
using ByteMe.Services.Interfaces;

namespace ByteMe.Services;

public class PaymentContext
{
    private IPaymentStrategy? _strategy;

    public void SetStrategy(IPaymentStrategy strategy)
    {
        _strategy = strategy;
    }

    public bool Execute(decimal amount)
    {
        if (_strategy == null)
            return false;
        return _strategy.ProcessPayment(amount);
    }

    public string? GetMethodName() => _strategy?.MethodName;
}

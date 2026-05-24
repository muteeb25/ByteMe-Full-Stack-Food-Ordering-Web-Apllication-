// PATTERN: STRATEGY (concrete)
using ByteMe.Services.Interfaces;

namespace ByteMe.Services.Patterns.PaymentStrategies;

public class CashOnDeliveryStrategy : IPaymentStrategy
{
    public string MethodName => "Cash on Delivery";

    public bool ProcessPayment(decimal amount) => true;
}

// PATTERN: STRATEGY (concrete)
using ByteMe.Services.Interfaces;

namespace ByteMe.Services.Patterns.PaymentStrategies;

public class CreditCardStrategy : IPaymentStrategy
{
    public string MethodName => "Credit Card";

    public bool ProcessPayment(decimal amount) => true;
}

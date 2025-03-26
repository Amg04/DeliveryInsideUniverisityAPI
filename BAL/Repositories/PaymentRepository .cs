using BAL.interfaces;
using BLLProject.Repositories;
using DAL.Data;
using DAL.Models;

// to implement adapter

public interface IPaymentGateway
{
    bool ProcessPayment(decimal amount);
}

public class PayPalPaymentAdapter : IPaymentGateway
{
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing PayPal payment of {amount}");
        return true;
    }
}

public class StripePaymentAdapter : IPaymentGateway
{
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing Stripe payment of {amount}");
        return true;
    }
}

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    private readonly IPaymentGateway _paymentGateway;

    public PaymentRepository(RestaurantAPIContext context, IPaymentGateway paymentGateway) : base(context)
    {
        _paymentGateway = paymentGateway;
    }

    public bool ProcessOrderPayment(Payment payment)
    {
        return _paymentGateway.ProcessPayment(payment.Order.TotalPrice);
    }
}


//IPaymentGateway paymentGateway = new PayPalPaymentAdapter(); // أو new StripePaymentAdapter()
//PaymentRepository paymentRepository = new PaymentRepository(dbContext, paymentGateway);


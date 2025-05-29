using DAL;

public class PaymentStatusUpdateRequest
{
    public int OrderId { get; set; }
    public PaymentStatus NewPaymentStatus { get; set; }
}
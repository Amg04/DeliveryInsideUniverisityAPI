using DAL;

public class OrderStatusUpdateRequest
{
    public int OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}


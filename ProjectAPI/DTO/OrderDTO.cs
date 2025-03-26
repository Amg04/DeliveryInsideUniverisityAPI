using DAL;
namespace ProjectAPI.DTO
{
    public class OrderDTO
    {
        public int id { get; set; }
        public string UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int EstablishmentId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public static class OrderExtensions
    {
        public static OrderDTO ToOrderDTO(this Order order)
        {
            return new OrderDTO
            {
                id = order.id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                ShippingDate = order.ShippingDate,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                EstablishmentId = order.EstablishmentId,
                Name = order.Name,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber
            };
        }

        public static Order ToOrderModel(this OrderDTO orderDTO, Order order = null)
        {
            if (order == null)
                order = new Order();

            order.id = orderDTO.id;
            order.UserId = orderDTO.UserId;
            order.TotalPrice = orderDTO.TotalPrice;
            order.OrderDate = orderDTO.OrderDate;
            order.ShippingDate = orderDTO.ShippingDate;
            order.OrderStatus = orderDTO.OrderStatus;
            order.PaymentStatus = orderDTO.PaymentStatus;
            order.EstablishmentId = orderDTO.EstablishmentId;
            order.Name = orderDTO.Name;
            order.Address = orderDTO.Address;
            order.PhoneNumber = orderDTO.PhoneNumber;

            return order;
        }
    }
}

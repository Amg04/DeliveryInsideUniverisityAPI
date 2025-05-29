using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace DAL.Configurations
{
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(u => u.TotalPrice)
            .HasColumnType("decimal(15,2)");

            var orderStatusConverter = new EnumToStringConverter<OrderStatus>();
            var paymentStatusConverter = new EnumToStringConverter<PaymentStatus>();

            builder.Property(e => e.OrderStatus).HasConversion(orderStatusConverter);
            builder.Property(e => e.PaymentStatus).HasConversion(paymentStatusConverter);


        }
    }
}

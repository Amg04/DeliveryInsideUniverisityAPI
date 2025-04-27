using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(u => u.Name)
               .HasColumnType("varchar(200)")
               .HasMaxLength(200)
               .IsRequired();

            builder.Property(u => u.ImageUrl)
              .HasColumnType("varchar(500)")
             .IsRequired();

            builder.Property(u => u.Description)
            .HasColumnType("varchar(500)")
            .IsRequired();

            builder.Property(u => u.price)
                .HasColumnType("decimal(10,2)");

        }
    }
}

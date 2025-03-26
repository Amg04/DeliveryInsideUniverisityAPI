using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DAL.Configurations
{
    public class CategoryConfigurations : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(u => u.Name)
               .HasColumnType("varchar(100)")
               .IsRequired();

            builder.Property(u => u.Description)
               .HasColumnType("varchar(100)")
               .IsRequired();
        }
    }
}

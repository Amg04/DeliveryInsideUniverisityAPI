using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    internal class ReviewConfigurations : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.Property(r => r.Comment)
                .HasColumnType("Text")
                .IsRequired();

            builder.Property(r => r.Rating)
               .IsRequired()
               .HasDefaultValue(3) 
               .HasAnnotation("MinValue", 1)
               .HasAnnotation("MaxValue", 5);
        }
    }
}

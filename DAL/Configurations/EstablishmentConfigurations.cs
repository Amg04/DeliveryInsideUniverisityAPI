using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class EstablishmentConfigurations : IEntityTypeConfiguration<Establishment>
    {
        public void Configure(EntityTypeBuilder<Establishment> builder)
        {
            builder.Property(r => r.Name)
            .HasColumnType("varchar(200)")
             .IsRequired();

            builder.Property(r => r.Location)
             .HasColumnType("varchar(400)")
             .IsRequired();

            builder.Property(r => r.OpeningHours)
            .HasColumnType("varchar(50)")
            .IsRequired();

            builder.Property(r => r.ContactNumber)
          .HasColumnType("varchar(200)")
          .IsRequired();

            builder.Property(e => e.EsbCategory)
                   .HasConversion(
                       v => v.ToString(),         
                       v => (EsbCategoryType)Enum.Parse(typeof(EsbCategoryType), v)
                   )
                   .IsRequired(); 

        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Payments_Amount", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_Payments_Method", "\"Method\" IN ('Efectivo','Tarjeta','Transferencia')");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.Amount).HasPrecision(12, 2);
        e.Property(x => x.Method).HasMaxLength(15).IsRequired();
        e.Property(x => x.Reference).HasMaxLength(60);
        e.HasIndex(x => new { x.EnterpriseId, x.ServiceOrderId });
        e.HasIndex(x => new { x.EnterpriseId, x.PaidAt });          // reporte de ingresos

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<ServiceOrder>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.ServiceOrderId })
            .HasPrincipalKey(o => new { o.EnterpriseId, o.Id })
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Employee>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.ReceivedByEmployeeId })
            .HasPrincipalKey(t => new { t.EnterpriseId, t.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ServiceOrders_Status", "\"Status\" IN ('Recibida','EnProceso','Cerrada')");
            t.HasCheckConstraint("CK_ServiceOrders_ClosedAt", "(\"Status\" = 'Cerrada') = (\"ClosedAt\" IS NOT NULL)");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.Status).HasMaxLength(15).IsRequired();
        e.Property(x => x.ReportedIssue).HasMaxLength(1000).IsRequired();
        e.Property(x => x.ReceivedCondition).HasMaxLength(1000);
        e.Property(x => x.WorkDone).HasMaxLength(2000);
        e.Property(x => x.Notes).HasMaxLength(2000);
        e.HasIndex(x => new { x.EnterpriseId, x.Status, x.CreatedAt });
        e.HasIndex(x => new { x.EnterpriseId, x.TechnicianId });

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        // FK compuestas: equipo, cliente y tecnico tienen que ser de la MISMA empresa
        e.HasOne<Equipment>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.EquipmentId })
            .HasPrincipalKey(q => new { q.EnterpriseId, q.Id })
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Customer>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.CustomerId })
            .HasPrincipalKey(c => new { c.EnterpriseId, c.Id })
            .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Employee>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.TechnicianId })
            .HasPrincipalKey(t => new { t.EnterpriseId, t.Id })
            .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ServiceItemConfiguration : IEntityTypeConfiguration<ServiceItem>
{
    public void Configure(EntityTypeBuilder<ServiceItem> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ServiceItems_Kind", "\"Kind\" IN ('Trabajo','Repuesto')");
            t.HasCheckConstraint("CK_ServiceItems_Quantity", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_ServiceItems_UnitPrice", "\"UnitPrice\" >= 0");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Kind).HasMaxLength(10).IsRequired();
        e.Property(x => x.Description).HasMaxLength(300).IsRequired();
        e.Property(x => x.UnitPrice).HasPrecision(12, 2);
        e.HasIndex(x => new { x.EnterpriseId, x.ServiceOrderId });
        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<ServiceOrder>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.ServiceOrderId })
            .HasPrincipalKey(o => new { o.EnterpriseId, o.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }
}

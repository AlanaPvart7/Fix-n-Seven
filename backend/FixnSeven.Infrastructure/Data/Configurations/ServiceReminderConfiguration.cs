using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class ServiceReminderConfiguration : IEntityTypeConfiguration<ServiceReminder>
{
    public void Configure(EntityTypeBuilder<ServiceReminder> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ServiceReminders_Due", "\"DueDate\" IS NOT NULL OR \"DueUsage\" IS NOT NULL");
            t.HasCheckConstraint("CK_ServiceReminders_DueUsage", "\"DueUsage\" IS NULL OR \"DueUsage\" > 0");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        e.HasIndex(x => new { x.EnterpriseId, x.DueDate });      // la lista de "proximos a vencer"
        e.HasIndex(x => new { x.EnterpriseId, x.EquipmentId });

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Equipment>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.EquipmentId })
            .HasPrincipalKey(q => new { q.EnterpriseId, q.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }
}

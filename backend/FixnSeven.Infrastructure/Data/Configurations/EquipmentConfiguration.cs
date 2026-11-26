using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class EquipmentCategoryConfiguration : IEntityTypeConfiguration<EquipmentCategory>
{
    public void Configure(EntityTypeBuilder<EquipmentCategory> e)
    {
        e.ToTable(t => t.HasCheckConstraint("CK_EquipmentCategories_UsageUnit", "\"UsageUnit\" IN ('Fechas','Horas')"));
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });          // destino de FK compuestas
        e.Property(x => x.Name).HasMaxLength(60).IsRequired();
        e.Property(x => x.IdentifierLabel).HasMaxLength(40).IsRequired();
        e.Property(x => x.UsageUnit).HasMaxLength(10).IsRequired();
        e.HasIndex(x => new { x.EnterpriseId, x.Name }).IsUnique();     // nombre unico por empresa
        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> e)
    {
        e.ToTable(t => t.HasCheckConstraint("CK_Equipment_CurrentUsage", "\"CurrentUsage\" IS NULL OR \"CurrentUsage\" >= 0"));
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.Identifier).HasMaxLength(60).IsRequired();
        e.Property(x => x.Brand).HasMaxLength(60).IsRequired();
        e.Property(x => x.Model).HasMaxLength(60).IsRequired();
        e.HasIndex(x => new { x.EnterpriseId, x.Identifier }).IsUnique();   // IMEI unico por empresa, no global

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);

        // FK COMPUESTAS: el cliente y la categoria tienen que ser de la MISMA empresa que el equipo
        e.HasOne<Customer>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.CustomerId })
            .HasPrincipalKey(c => new { c.EnterpriseId, c.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne<EquipmentCategory>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.CategoryId })
            .HasPrincipalKey(c => new { c.EnterpriseId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}

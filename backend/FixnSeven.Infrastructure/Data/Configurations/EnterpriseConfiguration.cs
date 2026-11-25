using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class EnterpriseConfiguration : IEntityTypeConfiguration<Enterprise>
{
    public void Configure(EntityTypeBuilder<Enterprise> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Name).HasMaxLength(120).IsRequired();
        e.Property(x => x.LogoUrl).HasMaxLength(500);
        e.HasOne<EnterpriseType>().WithMany().HasForeignKey(x => x.EnterpriseTypeId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Plan>().WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });   // para las FK compuestas de después
        e.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        e.Property(x => x.Email).HasMaxLength(254);
        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
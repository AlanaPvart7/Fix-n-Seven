using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        e.Property(x => x.Email).HasMaxLength(254).IsRequired();
        e.HasIndex(x => x.Email).IsUnique();                 // correo unico en TODO el sistema
        e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<RoleEmployee>().WithMany().HasForeignKey(x => x.RoleEmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}

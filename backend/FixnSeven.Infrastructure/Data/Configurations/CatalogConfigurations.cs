using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Common;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Name).HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Name).IsUnique();
        e.Property(x => x.MonthlyPriceUsd).HasPrecision(10, 2);
        e.HasData(
            new Plan { Id = SeedIds.PlanBasico,      Name = "Básico",      MonthlyPriceUsd = 1500, MaxUsers = 50,  MaxMonthlyOrders = 200 },
            new Plan { Id = SeedIds.PlanProfesional, Name = "Profesional", MonthlyPriceUsd = 2000, MaxUsers = 100, MaxMonthlyOrders = null },
            new Plan { Id = SeedIds.PlanTallerPlus,  Name = "Taller+",     MonthlyPriceUsd = 2500, MaxUsers = null, MaxMonthlyOrders = null });
    }
}

public class EnterpriseTypeConfiguration : IEntityTypeConfiguration<EnterpriseType>
{
    public void Configure(EntityTypeBuilder<EnterpriseType> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Code).HasMaxLength(40).IsRequired();
        e.HasIndex(x => x.Code).IsUnique();
        e.Property(x => x.Name).HasMaxLength(80).IsRequired();
        e.Property(x => x.Description).HasMaxLength(300);
        e.HasData(
            new EnterpriseType { Id = SeedIds.EnterpriseTypeWorkshop, Code = "Workshop",
                Name = "Taller de reparación", Description = "Recibe, repara y devuelve equipos", DefaultCertificateDays = 30 },
            new EnterpriseType { Id = SeedIds.EnterpriseTypeDistributor, Code = "UsedEquipmentDistributor",
                Name = "Distribuidor de equipos usados", Description = "Compra, revisa, reacondiciona y certifica equipos", DefaultCertificateDays = 30 });
    }
}

public class RoleEmployeeConfiguration : IEntityTypeConfiguration<RoleEmployee>
{
    public void Configure(EntityTypeBuilder<RoleEmployee> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Name).HasMaxLength(30).IsRequired();
        e.HasIndex(x => x.Name).IsUnique();
        e.HasData(
            new RoleEmployee { Id = SeedIds.RoleAdmin,      Name = "Admin" },
            new RoleEmployee { Id = SeedIds.RoleReception,  Name = "Reception" },
            new RoleEmployee { Id = SeedIds.RoleTechnician, Name = "Technician" });
    }
}
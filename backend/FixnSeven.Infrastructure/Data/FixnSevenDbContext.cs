using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Domain.Common;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data;

public class FixnSevenDbContext : DbContext
{
    private readonly ICurrentTenant _tenant;

    public FixnSevenDbContext(DbContextOptions<FixnSevenDbContext> options, ICurrentTenant tenant) : base(options)
    {
        _tenant = tenant;
    }

    // EF la evalua en CADA consulta, con el token de esa peticion
    private Guid? CurrentEnterpriseId => _tenant.EnterpriseId;

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<EnterpriseType> EnterpriseTypes => Set<EnterpriseType>();
    public DbSet<RoleEmployee> RoleEmployees => Set<RoleEmployee>();
    public DbSet<Enterprise> Enterprises => Set<Enterprise>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();
    public DbSet<Equipment> Equipment => Set<Equipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Lee todas las clases de la carpeta Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FixnSevenDbContext).Assembly);

        // Global Query Filters (HU-04): cada empresa solo ve SUS datos.
        // Agreguen una linea por cada tabla nueva que herede de TenantEntity.
        modelBuilder.Entity<Customer>().HasQueryFilter(x => x.EnterpriseId == CurrentEnterpriseId);
        modelBuilder.Entity<Employee>().HasQueryFilter(x => x.EnterpriseId == CurrentEnterpriseId);
        modelBuilder.Entity<EquipmentCategory>().HasQueryFilter(x => x.EnterpriseId == CurrentEnterpriseId);
        modelBuilder.Entity<Equipment>().HasQueryFilter(x => x.EnterpriseId == CurrentEnterpriseId);
    }

    // Al guardar: el EnterpriseId lo pone el sistema (desde el token) y nunca se puede cambiar
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.EnterpriseId == Guid.Empty)
            {
                entry.Entity.EnterpriseId = CurrentEnterpriseId
                    ?? throw new InvalidOperationException("No hay EnterpriseId en el token.");
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.EnterpriseId).IsModified = false;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
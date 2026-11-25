using Microsoft.EntityFrameworkCore;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data;

public class FixnSevenDbContext : DbContext
{
    public FixnSevenDbContext(DbContextOptions<FixnSevenDbContext> options) : base(options) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<EnterpriseType> EnterpriseTypes => Set<EnterpriseType>();
    public DbSet<RoleEmployee> RoleEmployees => Set<RoleEmployee>();
    public DbSet<Enterprise> Enterprises => Set<Enterprise>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Lee todas las clases de la carpeta Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FixnSevenDbContext).Assembly);
    }
}
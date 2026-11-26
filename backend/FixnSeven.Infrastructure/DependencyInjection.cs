using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Auth;
using FixnSeven.Application.Features.Customers;
using FixnSeven.Application.Features.Employees;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Enterprises;
using FixnSeven.Infrastructure.Data;
using FixnSeven.Infrastructure.Repositories;
using FixnSeven.Infrastructure.Security;

namespace FixnSeven.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // 1. Conexión a PostgreSQL
        services.AddDbContext<FixnSevenDbContext>(options => options.UseNpgsql(connectionString));

        // 2. "Cuando alguien pida ICustomerRepository, entrégale CustomerRepository"
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();

        // 3. Seguridad: contrasenas con BCrypt
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
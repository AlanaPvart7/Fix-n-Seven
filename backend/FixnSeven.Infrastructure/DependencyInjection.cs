using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FixnSeven.Application.Features.Customers;
using FixnSeven.Infrastructure.Data;
using FixnSeven.Infrastructure.Repositories;

namespace FixnSeven.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // 1. Conexión a PostgreSQL
        services.AddDbContext<FixnSevenDbContext>(options => options.UseNpgsql(connectionString));

        // 2. "Cuando alguien pida ICustomerRepository, entrégale CustomerRepository"
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
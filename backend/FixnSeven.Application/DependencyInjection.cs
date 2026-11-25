using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FixnSeven.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra automáticamente todos los validadores de esta capa
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
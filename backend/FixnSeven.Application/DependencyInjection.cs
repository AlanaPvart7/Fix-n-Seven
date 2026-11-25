using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FixnSeven.Application.Features.Auth;

namespace FixnSeven.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra automáticamente todos los validadores de esta capa
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Servicios con reglas de negocio
        services.AddScoped<AuthService>();
        return services;
    }
}
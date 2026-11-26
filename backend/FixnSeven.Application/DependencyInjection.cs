using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FixnSeven.Application.Features.Auth;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Employees;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Orders;
using FixnSeven.Application.Features.Reminders;
using FixnSeven.Application.Features.Settings;
using FixnSeven.Application.Features.Income;
using FixnSeven.Application.Features.Reports;
using FixnSeven.Application.Features.CustomerDevices;

namespace FixnSeven.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra automáticamente todos los validadores de esta capa
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Servicios con reglas de negocio
        services.AddScoped<AuthService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<EquipmentService>();
        services.AddScoped<OrderService>();
        services.AddScoped<CertificateService>();
        services.AddScoped<ReminderService>();
        services.AddScoped<PaymentService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<IncomeService>();
        services.AddScoped<ReportService>();
        services.AddScoped<CustomerAccountService>();
        return services;
    }
}
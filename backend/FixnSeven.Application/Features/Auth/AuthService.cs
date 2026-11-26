using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Auth.DTOs;
using FixnSeven.Application.Features.Enterprises.DTOs;
using FixnSeven.Domain.Common;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Auth;

// Reglas de negocio de HU-01 (registrar empresa) y HU-02 (iniciar sesion).
// Viven en Application: no saben nada de EF, BCrypt ni JWT; solo usan contratos.
public class AuthService
{
    private readonly IAuthRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IAuthRepository repository, IPasswordHasher hasher, ITokenService tokens)
    {
        _repository = repository;
        _hasher = hasher;
        _tokens = tokens;
    }

    private static readonly (string Name, string IdentifierLabel, string UsageUnit)[] DefaultCategories =
    {
        ("Celular", "IMEI", "Fechas"),
        ("Laptop", "N° de serie", "Fechas"),
        ("Televisor", "N° de serie", "Fechas"),
        ("Consola", "N° de serie", "Fechas"),
        ("Impresora", "N° de serie o código de activo", "Horas")
    };

    public async Task<AuthResult> RegisterEnterpriseAsync(RegisterEnterpriseRequestDto request)
    {
        var email = request.AdminEmail.Trim().ToLowerInvariant();

        if (await _repository.EmailExistsAsync(email))
            return AuthResult.Fail(409, "Ese correo ya está registrado.");

        var type = await _repository.GetEnterpriseTypeAsync(request.EnterpriseTypeId);
        if (type is null)
            return AuthResult.Fail(400, "El tipo de empresa no existe.");

        if (!await _repository.PlanExistsAsync(request.PlanId))
            return AuthResult.Fail(400, "El plan no existe.");

        var enterprise = new Enterprise
        {
            Name = request.EnterpriseName.Trim(),
            EnterpriseTypeId = type.Id,
            PlanId = request.PlanId,
            CertificateValidityDays = type.DefaultCertificateDays   // valor por defecto del tipo
        };

        var admin = new Employee
        {
            EnterpriseId = enterprise.Id,
            RoleEmployeeId = SeedIds.RoleAdmin,
            FullName = request.AdminFullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(request.Password)
        };

        // Categorias por defecto del rubro (la empresa luego puede agregar mas)
        var categories = DefaultCategories.Select(c => new EquipmentCategory
        {
            EnterpriseId = enterprise.Id,
            Name = c.Name,
            IdentifierLabel = c.IdentifierLabel,
            UsageUnit = c.UsageUnit
        });

        await _repository.AddEnterpriseWithAdminAsync(enterprise, admin, categories);   // una sola transaccion

        var (token, expiresAt) = _tokens.CreateToken(admin, "Admin");
        return AuthResult.Ok(new LoginResponseDto(token, expiresAt, "Admin", enterprise.Id), 201);
    }

    public async Task<AuthResult> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var found = await _repository.GetEmployeeWithRoleByEmailAsync(email);

        // Mismo mensaje si no existe el correo o si la contrasena esta mal (no damos pistas)
        if (found is null || !_hasher.Verify(request.Password, found.Value.Employee.PasswordHash))
            return AuthResult.Fail(401, "Correo o contraseña incorrectos.");

        var (employee, roleName) = found.Value;
        var (token, expiresAt) = _tokens.CreateToken(employee, roleName);
        return AuthResult.Ok(new LoginResponseDto(token, expiresAt, roleName, employee.EnterpriseId));
    }
}

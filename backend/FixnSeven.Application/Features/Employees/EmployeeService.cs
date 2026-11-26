using FixnSeven.Application.Common;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Employees.DTOs;
using FixnSeven.Domain.Common;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Employees;

// Reglas de negocio de HU-03: correo unico y limite de usuarios del plan
public class EmployeeService
{
    private static readonly Dictionary<string, Guid> RoleIds = new()
    {
        ["Admin"] = SeedIds.RoleAdmin,
        ["Reception"] = SeedIds.RoleReception,
        ["Technician"] = SeedIds.RoleTechnician
    };

    private readonly IEmployeeRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentTenant _tenant;

    public EmployeeService(IEmployeeRepository repository, IPasswordHasher hasher, ICurrentTenant tenant)
    {
        _repository = repository;
        _hasher = hasher;
        _tenant = tenant;
    }

    public async Task<ServiceResult<EmployeeResponseDto>> CreateAsync(CreateEmployeeRequestDto request)
    {
        var enterpriseId = _tenant.EnterpriseId
            ?? throw new InvalidOperationException("No hay EnterpriseId en el token.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email))
            return ServiceResult<EmployeeResponseDto>.Fail(409, "Ese correo ya está registrado.");

        var maxUsers = await _repository.GetPlanMaxUsersAsync(enterpriseId);
        if (maxUsers is not null && await _repository.CountAsync() >= maxUsers)
            return ServiceResult<EmployeeResponseDto>.Fail(409,
                $"Su plan permite como máximo {maxUsers} usuarios. Cambie de plan para agregar más.");

        var employee = new Employee
        {
            RoleEmployeeId = RoleIds[request.Role],
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(request.Password)
            // EnterpriseId: lo asigna SaveChanges desde el token
        };

        await _repository.AddAsync(employee);

        return ServiceResult<EmployeeResponseDto>.Ok(
            new EmployeeResponseDto(employee.Id, employee.FullName, employee.Email, request.Role), 201);
    }
}

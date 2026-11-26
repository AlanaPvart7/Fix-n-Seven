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

    // PUT /api/employees/{id}: cambia nombre y rol. La empresa nunca se queda sin Admin.
    public async Task<ServiceResult<bool>> UpdateAsync(Guid id, UpdateEmployeeRequestDto request)
    {
        var employee = await _repository.GetEntityAsync(id);
        if (employee is null) return ServiceResult<bool>.Fail(404, "El empleado no existe.");

        var newRoleId = RoleIds[request.Role];
        if (employee.RoleEmployeeId == SeedIds.RoleAdmin && newRoleId != SeedIds.RoleAdmin
            && await _repository.CountByRoleAsync(SeedIds.RoleAdmin) <= 1)
            return ServiceResult<bool>.Fail(409, "La empresa debe tener al menos un Admin.");

        employee.FullName = request.FullName.Trim();
        employee.RoleEmployeeId = newRoleId;
        await _repository.UpdateAsync(employee);
        return ServiceResult<bool>.Ok(true, 204);
    }
}

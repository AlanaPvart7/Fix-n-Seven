using FixnSeven.Application.Features.Employees.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Employees;

public interface IEmployeeRepository
{
    Task<bool> EmailExistsAsync(string email);                 // en TODO el sistema
    Task<int> CountAsync();                                    // solo de mi empresa
    Task<int?> GetPlanMaxUsersAsync(Guid enterpriseId);        // null = ilimitado
    Task AddAsync(Employee employee);
    Task<(List<EmployeeResponseDto> Items, int Total)> GetPagedAsync(string? role, int page, int pageSize);
    Task<EmployeeResponseDto?> GetByIdAsync(Guid id);
}

using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Employees;
using FixnSeven.Application.Features.Employees.DTOs;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly FixnSevenDbContext _context;
    public EmployeeRepository(FixnSevenDbContext context) => _context = context;

    // IgnoreQueryFilters: el correo es unico en todas las empresas, no solo en la mia
    public Task<bool> EmailExistsAsync(string email) =>
        _context.Employees.IgnoreQueryFilters().AnyAsync(e => e.Email == email);

    public Task<int> CountAsync() => _context.Employees.CountAsync();   // filtrado por empresa

    public Task<int?> GetPlanMaxUsersAsync(Guid enterpriseId) =>
        (from e in _context.Enterprises
         join p in _context.Plans on e.PlanId equals p.Id
         where e.Id == enterpriseId
         select p.MaxUsers).FirstOrDefaultAsync();

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<EmployeeResponseDto> Items, int Total)> GetPagedAsync(string? role, int page, int pageSize)
    {
        var query = from e in _context.Employees
                    join r in _context.RoleEmployees on e.RoleEmployeeId equals r.Id
                    select new { e.Id, e.FullName, e.Email, Role = r.Name };

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(x => x.Role == role);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EmployeeResponseDto(x.Id, x.FullName, x.Email, x.Role))
            .ToListAsync();

        return (items, total);
    }

    public Task<EmployeeResponseDto?> GetByIdAsync(Guid id) =>
        (from e in _context.Employees
         join r in _context.RoleEmployees on e.RoleEmployeeId equals r.Id
         where e.Id == id
         select new EmployeeResponseDto(e.Id, e.FullName, e.Email, r.Name))
        .FirstOrDefaultAsync();
}

using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Auth;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly FixnSevenDbContext _context;
    public AuthRepository(FixnSevenDbContext context) => _context = context;

    // IgnoreQueryFilters: en el login todavia no hay token, asi que se busca en todas las empresas
    public Task<bool> EmailExistsAsync(string email) =>
        _context.Employees.IgnoreQueryFilters().AnyAsync(e => e.Email == email);

    public Task<EnterpriseType?> GetEnterpriseTypeAsync(Guid id) =>
        _context.EnterpriseTypes.FirstOrDefaultAsync(t => t.Id == id);

    public Task<bool> PlanExistsAsync(Guid id) =>
        _context.Plans.AnyAsync(p => p.Id == id);

    public async Task AddEnterpriseWithAdminAsync(Enterprise enterprise, Employee admin, IEnumerable<EquipmentCategory> categories)
    {
        _context.Enterprises.Add(enterprise);
        _context.Employees.Add(admin);
        _context.EquipmentCategories.AddRange(categories);
        await _context.SaveChangesAsync();   // las dos filas se guardan juntas o ninguna
    }

    public async Task<(Employee Employee, string RoleName)?> GetEmployeeWithRoleByEmailAsync(string email)
    {
        var row = await _context.Employees.IgnoreQueryFilters()
            .Where(e => e.Email == email)
            .Join(_context.RoleEmployees, e => e.RoleEmployeeId, r => r.Id,
                  (e, r) => new { Employee = e, RoleName = r.Name })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.Employee, row.RoleName);
    }
}

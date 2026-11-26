using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Auth;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<EnterpriseType?> GetEnterpriseTypeAsync(Guid id);
    Task<bool> PlanExistsAsync(Guid id);
    Task AddEnterpriseWithAdminAsync(Enterprise enterprise, Employee admin, IEnumerable<EquipmentCategory> categories);
    Task<(Employee Employee, string RoleName)?> GetEmployeeWithRoleByEmailAsync(string email);
}

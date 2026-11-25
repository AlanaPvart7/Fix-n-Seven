using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public class Employee : TenantEntity
{
    public Guid RoleEmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;          // se guarda en minusculas
    public string PasswordHash { get; set; } = string.Empty;   // BCrypt, nunca texto plano
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

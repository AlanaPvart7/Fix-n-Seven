using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public class Customer : TenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}
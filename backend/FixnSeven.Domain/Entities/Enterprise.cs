using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public class Enterprise : BaseEntity   // el inquilino: NO lleva EnterpriseId
{
    public Guid EnterpriseTypeId { get; set; }
    public Guid PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int CertificateValidityDays { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
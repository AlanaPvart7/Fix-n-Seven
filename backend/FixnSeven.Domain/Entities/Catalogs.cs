using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPriceUsd { get; set; }
    public int? MaxUsers { get; set; }          // null = ilimitado
    public int? MaxMonthlyOrders { get; set; }  // null = ilimitado
}

public class EnterpriseType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefaultCertificateDays { get; set; } = 30;
}

public class RoleEmployee : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
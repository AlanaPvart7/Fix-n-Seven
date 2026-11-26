using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public class ServiceItem : TenantEntity
{
    public Guid ServiceOrderId { get; set; }
    public string Kind { get; set; } = "Trabajo";   // Trabajo o Repuesto
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }          // precio pactado en ESA orden
}

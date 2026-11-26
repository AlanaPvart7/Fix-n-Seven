using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

// Rubro que atiende la empresa: Celular, Laptop, Televisor...
public class EquipmentCategory : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string IdentifierLabel { get; set; } = string.Empty;   // IMEI, N° de serie...
    public string UsageUnit { get; set; } = "Fechas";             // Fechas u Horas
}

using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public class Equipment : TenantEntity
{
    public Guid? CustomerId { get; set; }        // null = equipo propio del distribuidor
    public Guid CategoryId { get; set; }
    public string Identifier { get; set; } = string.Empty;   // IMEI o numero de serie, unico por empresa
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? CurrentUsage { get; set; }       // horas de uso, si la categoria mide por horas
}

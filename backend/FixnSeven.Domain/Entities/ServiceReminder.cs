using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

// HU-17: proximo servicio de un equipo, por fecha o por horas de uso
public class ServiceReminder : TenantEntity
{
    public Guid EquipmentId { get; set; }
    public string Reason { get; set; } = string.Empty;   // "Vence la garantia", "Limpieza y pasta termica"...
    public DateOnly? DueDate { get; set; }               // DueDate o DueUsage: al menos uno (CHECK)
    public int? DueUsage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

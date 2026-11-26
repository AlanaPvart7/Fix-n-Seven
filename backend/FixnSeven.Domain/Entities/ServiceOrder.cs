using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public static class OrderStatus
{
    public const string Received = "Recibida";     // creada, sin tecnico
    public const string InProgress = "EnProceso";  // con tecnico asignado
    public const string Closed = "Cerrada";
}

public class ServiceOrder : TenantEntity
{
    public Guid EquipmentId { get; set; }
    public Guid? CustomerId { get; set; }          // quien trajo el equipo EN ESE MOMENTO (el dueno puede cambiar)
    public Guid? TechnicianId { get; set; }        // null hasta que se asigna
    public string Status { get; set; } = OrderStatus.Received;
    public string ReportedIssue { get; set; } = string.Empty;
    public string? ReceivedCondition { get; set; }
    public string? WorkDone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    // El TOTAL no se guarda: se calcula sumando los ServiceItem (3FN)
}

namespace FixnSeven.Application.Features.Orders.DTOs;

public record CreateOrderRequestDto(
    Guid EquipmentId,
    Guid? TechnicianId,          // opcional: se puede asignar despues
    string ReportedIssue,        // problema que reporta el cliente
    string? ReceivedCondition    // como llega el equipo (rayones, accesorios...)
);

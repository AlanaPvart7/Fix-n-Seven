using FixnSeven.Application.Features.Equipment.DTOs;
namespace FixnSeven.Application.Features.Orders.DTOs;

public record OrderDetailResponseDto(
    Guid Id,
    string Status,
    EquipmentResponseDto Equipment,
    string? CustomerName,
    Guid? TechnicianId,
    string? TechnicianName,
    string ReportedIssue,
    string? ReceivedCondition,
    string? WorkDone,
    string? Notes,
    List<ServiceItemResponseDto> Items,
    decimal Total,                 // suma de los items, no se guarda
    DateTime CreatedAt,
    DateTime? ClosedAt
);

using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.Application.Features.Equipment.DTOs;

// HU-07: historial del equipo (sus ordenes, de la mas reciente a la mas vieja)
public record EquipmentDetailResponseDto(
    EquipmentResponseDto Equipment,
    List<OrderSummaryResponseDto> Orders
);

namespace FixnSeven.Application.Features.Income.DTOs;

public record IncomeByTechnicianResponseDto(
    Guid? TechnicianId,
    string TechnicianName,
    int ClosedOrders,
    decimal Revenue
);

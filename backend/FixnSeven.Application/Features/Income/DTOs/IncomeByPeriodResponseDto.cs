namespace FixnSeven.Application.Features.Income.DTOs;

public record IncomeByPeriodResponseDto(
    DateOnly PeriodStart,   // el dia, o el dia 1 del mes
    int ClosedOrders,
    decimal Revenue,
    decimal Collected
);

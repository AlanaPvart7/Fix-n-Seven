namespace FixnSeven.Application.Features.Income.DTOs;

// HU-21 (Fase 2): todo se calcula a partir de ordenes cerradas y pagos; nada se guarda
public record IncomeReportResponseDto(
    DateOnly From,
    DateOnly To,
    string GroupBy,                 // day o month
    decimal TotalRevenue,           // suma de ordenes cerradas en el periodo
    decimal TotalCollected,         // suma de pagos recibidos en el periodo
    int ClosedOrders,
    List<IncomeByTechnicianResponseDto> ByTechnician,
    List<IncomeByPeriodResponseDto> ByPeriod
);

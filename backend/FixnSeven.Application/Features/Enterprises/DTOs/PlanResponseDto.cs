namespace FixnSeven.Application.Features.Enterprises.DTOs;

public record PlanResponseDto(Guid Id, string Name, decimal MonthlyPriceUsd, int? MaxUsers, int? MaxMonthlyOrders);

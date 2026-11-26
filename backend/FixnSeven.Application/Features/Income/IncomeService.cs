using FixnSeven.Application.Common;
using FixnSeven.Application.Features.Income.DTOs;
namespace FixnSeven.Application.Features.Income;

// HU-21 (Fase 2): ingresos por periodo y por tecnico
public class IncomeService
{
    private readonly IIncomeRepository _repository;
    public IncomeService(IIncomeRepository repository) => _repository = repository;

    public async Task<ServiceResult<IncomeReportResponseDto>> GetReportAsync(DateOnly? from, DateOnly? to, string? groupBy)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = from ?? new DateOnly(today.Year, today.Month, 1);   // por defecto: este mes
        var end = to ?? today;
        groupBy = string.IsNullOrWhiteSpace(groupBy) ? "day" : groupBy.Trim().ToLowerInvariant();

        if (groupBy is not ("day" or "month"))
            return ServiceResult<IncomeReportResponseDto>.Fail(400, "groupBy debe ser day o month.");
        if (start > end)
            return ServiceResult<IncomeReportResponseDto>.Fail(400, "La fecha inicial no puede ser mayor que la final.");
        if (end.DayNumber - start.DayNumber > 366)
            return ServiceResult<IncomeReportResponseDto>.Fail(400, "El rango máximo es de un año.");

        var fromUtc = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = end.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);   // incluye todo el dia final

        var orders = await _repository.GetClosedOrdersAsync(fromUtc, toUtc);
        var payments = await _repository.GetPaymentsAsync(fromUtc, toUtc);

        DateOnly Period(DateTime utc)
        {
            var d = DateOnly.FromDateTime(utc);
            return groupBy == "month" ? new DateOnly(d.Year, d.Month, 1) : d;
        }

        var byTechnician = orders
            .GroupBy(o => new { o.TechnicianId, o.TechnicianName })
            .Select(g => new IncomeByTechnicianResponseDto(
                g.Key.TechnicianId, g.Key.TechnicianName ?? "Sin técnico", g.Count(), g.Sum(o => o.Total)))
            .OrderByDescending(t => t.Revenue)
            .ToList();

        var periods = orders.Select(o => Period(o.ClosedAt))
            .Concat(payments.Select(p => Period(p.PaidAt)))
            .Distinct().OrderBy(d => d);
        var byPeriod = periods.Select(p => new IncomeByPeriodResponseDto(
                p,
                orders.Count(o => Period(o.ClosedAt) == p),
                orders.Where(o => Period(o.ClosedAt) == p).Sum(o => o.Total),
                payments.Where(x => Period(x.PaidAt) == p).Sum(x => x.Amount)))
            .ToList();

        return ServiceResult<IncomeReportResponseDto>.Ok(new IncomeReportResponseDto(
            start, end, groupBy,
            orders.Sum(o => o.Total),
            payments.Sum(p => p.Amount),
            orders.Count,
            byTechnician,
            byPeriod));
    }
}

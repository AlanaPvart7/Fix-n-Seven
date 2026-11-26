using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Enterprises;
using FixnSeven.Application.Features.Enterprises.DTOs;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly FixnSevenDbContext _context;
    public CatalogRepository(FixnSevenDbContext context) => _context = context;

    public Task<List<EnterpriseTypeResponseDto>> GetEnterpriseTypesAsync() =>
        _context.EnterpriseTypes
            .OrderBy(t => t.Name)
            .Select(t => new EnterpriseTypeResponseDto(t.Id, t.Code, t.Name, t.Description, t.DefaultCertificateDays))
            .ToListAsync();

    public Task<List<PlanResponseDto>> GetPlansAsync() =>
        _context.Plans
            .OrderBy(p => p.MonthlyPriceUsd)
            .Select(p => new PlanResponseDto(p.Id, p.Name, p.MonthlyPriceUsd, p.MaxUsers, p.MaxMonthlyOrders))
            .ToListAsync();

    public Task<EnterpriseResponseDto?> GetEnterpriseAsync(Guid enterpriseId) =>
        (from e in _context.Enterprises
         join t in _context.EnterpriseTypes on e.EnterpriseTypeId equals t.Id
         join p in _context.Plans on e.PlanId equals p.Id
         where e.Id == enterpriseId
         select new EnterpriseResponseDto(e.Id, e.Name, t.Name, p.Name, e.LogoUrl, e.CertificateValidityDays))
        .FirstOrDefaultAsync();
}

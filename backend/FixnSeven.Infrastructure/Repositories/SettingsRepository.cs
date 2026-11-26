using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Settings;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly FixnSevenDbContext _context;
    public SettingsRepository(FixnSevenDbContext context) => _context = context;

    public Task<int> GetValidityDaysAsync(Guid enterpriseId) =>
        _context.Enterprises.Where(e => e.Id == enterpriseId)
            .Select(e => e.CertificateValidityDays).FirstAsync();

    public Task<List<CertificateGradeScale>> GetScaleAsync() =>
        _context.CertificateGradeScales.AsNoTracking().ToListAsync();

    // Reemplaza la escala completa y la vigencia en una sola transaccion
    public async Task SaveAsync(Guid enterpriseId, int validityDays, List<CertificateGradeScale> scale)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var enterprise = await _context.Enterprises.FirstAsync(e => e.Id == enterpriseId);
        enterprise.CertificateValidityDays = validityDays;

        _context.CertificateGradeScales.RemoveRange(await _context.CertificateGradeScales.ToListAsync());
        await _context.SaveChangesAsync();

        await _context.CertificateGradeScales.AddRangeAsync(scale);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

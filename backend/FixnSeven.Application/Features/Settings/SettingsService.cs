using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Settings.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Settings;

// Fase 2: ajustes de certificados (vigencia y escala de calidad de cada empresa)
public class SettingsService
{
    private readonly ISettingsRepository _repository;
    private readonly ICurrentTenant _tenant;

    public SettingsService(ISettingsRepository repository, ICurrentTenant tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    private Guid EnterpriseId => _tenant.EnterpriseId ?? throw new InvalidOperationException("No hay EnterpriseId en el token.");

    public static List<GradeScaleItemDto> DefaultScale() =>
        CertificateGrades.All
            .Select(g => new GradeScaleItemDto(g, CertificateGrades.Range(g).Min, CertificateGrades.Range(g).Max))
            .OrderByDescending(i => i.MinScore).ToList();

    // La escala que se aplica al emitir: la de la empresa, o la inicial si nunca la cambio
    public async Task<List<GradeScaleItemDto>> GetEffectiveScaleAsync()
    {
        var rows = await _repository.GetScaleAsync();
        return rows.Count == 0
            ? DefaultScale()
            : rows.Select(r => new GradeScaleItemDto(r.Grade, r.MinScore, r.MaxScore))
                  .OrderByDescending(i => i.MinScore).ToList();
    }

    public async Task<CertificateSettingsResponseDto> GetAsync()
    {
        var rows = await _repository.GetScaleAsync();
        return new CertificateSettingsResponseDto(
            await _repository.GetValidityDaysAsync(EnterpriseId),
            await GetEffectiveScaleAsync(),
            rows.Count == 0);
    }

    public Task UpdateAsync(UpdateCertificateSettingsRequestDto request) =>
        _repository.SaveAsync(EnterpriseId, request.ValidityDays,
            request.GradeScale.Select(i => new CertificateGradeScale
            {
                Grade = i.Grade, MinScore = i.MinScore, MaxScore = i.MaxScore
            }).ToList());
}

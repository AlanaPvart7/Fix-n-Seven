using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Settings;

public interface ISettingsRepository
{
    Task<int> GetValidityDaysAsync(Guid enterpriseId);
    Task<List<CertificateGradeScale>> GetScaleAsync();          // de mi empresa (vacia = escala inicial)
    Task SaveAsync(Guid enterpriseId, int validityDays, List<CertificateGradeScale> scale);   // una transaccion
}

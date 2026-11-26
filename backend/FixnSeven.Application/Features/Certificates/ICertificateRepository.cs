using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Certificates;

public interface ICertificateRepository
{
    Task<bool> EquipmentExistsAsync(Guid equipmentId);
    Task<bool> VerificationCodeExistsAsync(string code);       // en TODO el sistema
    Task<int> GetValidityDaysAsync(Guid enterpriseId);
    Task<string> GetEmployeeNameAsync(Guid employeeId);

    Task<Certificate?> GetEntityAsync(Guid id);
    Task<Certificate?> GetActiveEntityByEquipmentAsync(Guid equipmentId);
    Task IssueAsync(Certificate certificate, Certificate? expiredToClose);   // en una transaccion
    Task UpdateAsync(Certificate certificate);

    Task<CertificateResponseDto?> GetByIdAsync(Guid id);
    Task<List<CertificateResponseDto>> GetByEquipmentAsync(Guid equipmentId);
    Task<CertificateResponseDto?> GetActiveByEquipmentAsync(Guid equipmentId);
    Task<CertificateResponseDto?> GetLatestByOrderAsync(Guid orderId);

    Task<PublicCertificateResponseDto?> GetPublicAsync(string verificationCode);   // sin token
}

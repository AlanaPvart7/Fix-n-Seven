namespace FixnSeven.Application.Features.Enterprises.DTOs;

public record EnterpriseResponseDto(
    Guid Id,
    string Name,
    string EnterpriseType,
    string Plan,
    string? LogoUrl,
    int CertificateValidityDays
);

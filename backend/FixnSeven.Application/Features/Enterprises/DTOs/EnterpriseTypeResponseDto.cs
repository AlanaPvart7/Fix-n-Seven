namespace FixnSeven.Application.Features.Enterprises.DTOs;

public record EnterpriseTypeResponseDto(Guid Id, string Code, string Name, string Description, int DefaultCertificateDays);

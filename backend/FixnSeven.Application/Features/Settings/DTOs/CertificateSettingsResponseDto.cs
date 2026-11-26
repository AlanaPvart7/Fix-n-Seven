namespace FixnSeven.Application.Features.Settings.DTOs;

public record CertificateSettingsResponseDto(
    int ValidityDays,
    List<GradeScaleItemDto> GradeScale,
    bool IsDefaultScale          // true = la empresa todavia usa la escala inicial
);

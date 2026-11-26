namespace FixnSeven.Application.Features.Settings.DTOs;

public record UpdateCertificateSettingsRequestDto(
    int ValidityDays,
    List<GradeScaleItemDto> GradeScale   // los 4 niveles, sin huecos ni traslapes, de 0 a 100
);

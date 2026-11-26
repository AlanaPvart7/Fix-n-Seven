namespace FixnSeven.Application.Features.Reports.DTOs;

public record CreateReportRequestDto(
    string Reason,       // EstadoDistinto, NivelIncorrecto, DatosErroneos, PosibleFalsificacion u Otro
    string Description,  // al menos 20 caracteres
    string ReporterName,
    string ReporterEmail,
    string? ReporterPhone
);

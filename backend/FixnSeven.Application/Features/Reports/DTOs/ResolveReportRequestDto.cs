namespace FixnSeven.Application.Features.Reports.DTOs;

public record ResolveReportRequestDto(
    string Decision,        // Aceptado o Rechazado
    string Resolution,      // explicacion obligatoria
    bool RevokeCertificate  // solo si se acepta
);

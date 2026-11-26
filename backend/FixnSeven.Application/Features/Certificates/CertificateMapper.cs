using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Certificates;

public static class CertificateMapper
{
    public const string Disclaimer =
        "Este certificado solo afirma que un tecnico identificado inspecciono el equipo en esa fecha y obtuvo ese resultado. " +
        "No garantiza el precio, la propiedad, el origen legal ni el funcionamiento futuro del equipo. " +
        "Fix n' Seven no participa en la compra, la venta ni los precios.";

    public static CertificateResponseDto ToDto(Certificate c, string technicianName,
        string? ownerCode = null, List<string>? warnings = null) =>
        new(c.Id, c.EquipmentId, c.ServiceOrderId, c.Grade, c.Score, c.CurrentStatus(DateTime.UtcNow),
            c.VerificationCode, c.TechnicianId, technicianName, c.IssuedAt, c.ExpiresAt,
            c.RevokedAt, c.RevocationReason, c.Observations, c.PublicSummary, ownerCode, warnings);

    // "356789012345678" -> "****5678"
    public static string Mask(string identifier) =>
        identifier.Length <= 4 ? "****" : "****" + identifier[^4..];
}

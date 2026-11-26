using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public static class CertificateStatus
{
    public const string Active = "Vigente";
    public const string Revoked = "Revocado";
    public const string Expired = "Vencido";   // NO se guarda: se calcula con ExpiresAt (3FN)
}

// Escala inicial de calidad (en Fase 2 cada empresa la podra ajustar)
public static class CertificateGrades
{
    public const string Excellent = "Excelente";
    public const string Good = "Bueno";
    public const string Fair = "Regular";
    public const string Failed = "Reprobado";

    private static readonly Dictionary<string, (int Min, int Max)> Scale = new()
    {
        [Excellent] = (90, 100),
        [Good] = (75, 89),
        [Fair] = (60, 74),
        [Failed] = (0, 59)
    };

    public static IReadOnlyCollection<string> All => Scale.Keys;
    public static bool IsValid(string? grade) => grade is not null && Scale.ContainsKey(grade);
    public static (int Min, int Max) Range(string grade) => Scale[grade];
    public static bool Fits(string grade, int score) =>
        IsValid(grade) && score >= Scale[grade].Min && score <= Scale[grade].Max;
}

public class Certificate : TenantEntity
{
    public Guid EquipmentId { get; set; }
    public Guid? ServiceOrderId { get; set; }      // orden que origino la inspeccion (opcional)
    public Guid TechnicianId { get; set; }         // sale del JWT, nunca del body
    public string Grade { get; set; } = string.Empty;   // foto del nivel al emitir (excepcion justificada a 3FN)
    public int Score { get; set; }
    public string? Observations { get; set; }      // notas internas, nunca publicas
    public string? PublicSummary { get; set; }     // lo unico de las notas que ve el publico
    public string Status { get; set; } = CertificateStatus.Active;
    public string VerificationCode { get; set; } = string.Empty;   // va en el QR
    public string OwnerCodeHash { get; set; } = string.Empty;      // huella del codigo privado del dueno
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }

    // Vigente, Vencido o Revocado, calculado al leer
    public string CurrentStatus(DateTime nowUtc) =>
        Status == CertificateStatus.Revoked ? CertificateStatus.Revoked
        : ExpiresAt < nowUtc ? CertificateStatus.Expired
        : CertificateStatus.Active;
}

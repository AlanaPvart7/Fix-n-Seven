using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

// Cuenta GLOBAL del comprador final: no pertenece a ningun taller (sin EnterpriseId).
// No esta enlazada a la ficha Customer, asi ninguna empresa ve sus datos (regla 16).
public class CustomerAccount : BaseEntity
{
    public string Email { get; set; } = string.Empty;          // en minusculas, unico
    public string PasswordHash { get; set; } = string.Empty;   // BCrypt
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int FailedLinkAttempts { get; set; }                // 5 fallos seguidos = bloqueo de 15 min
    public DateTime? LinkLockedUntil { get; set; }
}

public static class DeviceStatus
{
    public const string Active = "Activo";
    public const string Transferred = "Transferido";
}

// Equipo guardado en la cuenta del comprador. Un equipo tiene como maximo UN dueno Activo.
// No guarda CertificateId: el certificado se obtiene desde EquipmentId (3FN).
public class CustomerDevice : BaseEntity
{
    public Guid CustomerAccountId { get; set; }
    public Guid EquipmentId { get; set; }
    public string? Alias { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public string? Accessories { get; set; }
    public string? PublicDescription { get; set; }      // maximo 500
    public string? PrivateNotes { get; set; }           // nunca se muestran
    public bool ShowInPublicView { get; set; }          // falso por defecto
    public string Status { get; set; } = DeviceStatus.Active;
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public string? TransferCodeHash { get; set; }       // codigo de transferencia de un solo uso
    public DateTime? TransferExpiresAt { get; set; }
}

public class CustomerDeviceFile : BaseEntity
{
    public Guid CustomerDeviceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileKind { get; set; } = FileKinds.Photo;   // Foto o PDF
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsPublic { get; set; }                         // solo las fotos pueden ser publicas
    public string StorageKey { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

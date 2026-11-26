using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

public static class ReportStatus
{
    public const string Received = "Recibido";
    public const string InReview = "EnRevision";
    public const string Accepted = "Aceptado";
    public const string Rejected = "Rechazado";
}

public static class ReportReasons
{
    public static readonly string[] All =
        { "EstadoDistinto", "NivelIncorrecto", "DatosErroneos", "PosibleFalsificacion", "Otro" };
}

public static class ReporterTypes
{
    public const string Customer = "Cliente";
    public const string Distributor = "Distribuidor";
}

// Reporte de un certificado incorrecto. EnterpriseId = la empresa que EMITIO el certificado:
// solo ella lo ve y lo resuelve. Quien reporta no necesita cuenta.
public class Report : TenantEntity
{
    public Guid CertificateId { get; set; }
    public string ReporterType { get; set; } = ReporterTypes.Customer;
    public Guid? ReporterEnterpriseId { get; set; }      // si reporto con sesion de otra empresa
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string? ReporterPhone { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = ReportStatus.Received;
    public string TrackingCode { get; set; } = string.Empty;   // para consultar sin cuenta
    public string? Resolution { get; set; }
    public Guid? ResolvedByEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public static class FileKinds
{
    public const string Photo = "Foto";
    public const string Pdf = "PDF";
    public const string Video = "Video";
    public const string Document = "Documento";
}

// Prueba adjunta: el archivo va al almacenamiento privado, aqui solo la referencia
public class ReportAttachment : TenantEntity
{
    public Guid ReportId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileKind { get; set; } = FileKinds.Photo;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

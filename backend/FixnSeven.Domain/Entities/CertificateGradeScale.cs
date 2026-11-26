using FixnSeven.Domain.Common;
namespace FixnSeven.Domain.Entities;

// Escala de calidad propia de cada empresa (si no tiene filas, se usa la escala inicial)
public class CertificateGradeScale : TenantEntity
{
    public string Grade { get; set; } = string.Empty;   // Excelente, Bueno, Regular o Reprobado
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
}

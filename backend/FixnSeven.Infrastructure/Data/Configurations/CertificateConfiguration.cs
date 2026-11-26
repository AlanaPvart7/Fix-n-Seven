using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Certificates_Status", "\"Status\" IN ('Vigente','Revocado')");
            t.HasCheckConstraint("CK_Certificates_Grade", "\"Grade\" IN ('Excelente','Bueno','Regular','Reprobado')");
            t.HasCheckConstraint("CK_Certificates_Score", "\"Score\" BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_Certificates_Expires", "\"ExpiresAt\" > \"IssuedAt\"");
            t.HasCheckConstraint("CK_Certificates_Revoked", "(\"Status\" = 'Revocado') = (\"RevokedAt\" IS NOT NULL)");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.Grade).HasMaxLength(15).IsRequired();
        e.Property(x => x.Status).HasMaxLength(15).IsRequired();
        e.Property(x => x.Observations).HasMaxLength(2000);
        e.Property(x => x.PublicSummary).HasMaxLength(500);
        e.Property(x => x.VerificationCode).HasMaxLength(20).IsRequired();
        e.Property(x => x.OwnerCodeHash).HasMaxLength(100).IsRequired();
        e.Property(x => x.RevocationReason).HasMaxLength(500);

        // El QR busca por este codigo: unico en TODO el sistema
        e.HasIndex(x => x.VerificationCode).IsUnique();
        // Maximo UN certificado vigente por equipo (indice unico filtrado)
        e.HasIndex(x => x.EquipmentId).IsUnique().HasFilter("\"Status\" = 'Vigente'");
        e.HasIndex(x => new { x.EnterpriseId, x.TechnicianId });
        e.HasIndex(x => new { x.EnterpriseId, x.ServiceOrderId });

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        // FK compuestas: equipo, orden y tecnico tienen que ser de la MISMA empresa
        e.HasOne<Equipment>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.EquipmentId })
            .HasPrincipalKey(q => new { q.EnterpriseId, q.Id })
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne<ServiceOrder>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.ServiceOrderId })
            .HasPrincipalKey(o => new { o.EnterpriseId, o.Id })
            .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Employee>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.TechnicianId })
            .HasPrincipalKey(t => new { t.EnterpriseId, t.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}

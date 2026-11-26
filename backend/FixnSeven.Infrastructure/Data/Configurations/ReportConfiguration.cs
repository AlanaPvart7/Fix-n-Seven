using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Reports_Status", "\"Status\" IN ('Recibido','EnRevision','Aceptado','Rechazado')");
            t.HasCheckConstraint("CK_Reports_Reason",
                "\"Reason\" IN ('EstadoDistinto','NivelIncorrecto','DatosErroneos','PosibleFalsificacion','Otro')");
            t.HasCheckConstraint("CK_Reports_ReporterType", "\"ReporterType\" IN ('Cliente','Distribuidor')");
            t.HasCheckConstraint("CK_Reports_Resolved",
                "(\"Status\" IN ('Aceptado','Rechazado')) = (\"ResolvedAt\" IS NOT NULL)");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        e.Property(x => x.ReporterType).HasMaxLength(15).IsRequired();
        e.Property(x => x.ReporterName).HasMaxLength(120).IsRequired();
        e.Property(x => x.ReporterEmail).HasMaxLength(254).IsRequired();
        e.Property(x => x.ReporterPhone).HasMaxLength(30);
        e.Property(x => x.Reason).HasMaxLength(25).IsRequired();
        e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        e.Property(x => x.Status).HasMaxLength(15).IsRequired();
        e.Property(x => x.TrackingCode).HasMaxLength(20).IsRequired();
        e.Property(x => x.Resolution).HasMaxLength(2000);

        e.HasIndex(x => x.TrackingCode).IsUnique();
        e.HasIndex(x => new { x.EnterpriseId, x.Status });
        e.HasIndex(x => x.CertificateId);
        e.HasIndex(x => new { x.ReporterEmail, x.CreatedAt });   // limite de 3 reportes por dia

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.ReporterEnterpriseId)
            .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        // FK compuestas: el certificado y quien resuelve son de la MISMA empresa del reporte
        e.HasOne<Certificate>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.CertificateId })
            .HasPrincipalKey(c => new { c.EnterpriseId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Employee>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.ResolvedByEmployeeId })
            .HasPrincipalKey(t => new { t.EnterpriseId, t.Id })
            .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReportAttachmentConfiguration : IEntityTypeConfiguration<ReportAttachment>
{
    public void Configure(EntityTypeBuilder<ReportAttachment> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ReportAttachments_Kind", "\"FileKind\" IN ('Foto','PDF','Video','Documento')");
            t.HasCheckConstraint("CK_ReportAttachments_Size", "\"SizeBytes\" > 0");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        e.Property(x => x.FileKind).HasMaxLength(15).IsRequired();
        e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        e.Property(x => x.StorageKey).HasMaxLength(300).IsRequired();
        e.HasIndex(x => new { x.EnterpriseId, x.ReportId });

        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Report>().WithMany()
            .HasForeignKey(x => new { x.EnterpriseId, x.ReportId })
            .HasPrincipalKey(r => new { r.EnterpriseId, r.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }
}

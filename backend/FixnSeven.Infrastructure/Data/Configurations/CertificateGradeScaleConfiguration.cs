using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class CertificateGradeScaleConfiguration : IEntityTypeConfiguration<CertificateGradeScale>
{
    public void Configure(EntityTypeBuilder<CertificateGradeScale> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_GradeScale_Grade", "\"Grade\" IN ('Excelente','Bueno','Regular','Reprobado')");
            t.HasCheckConstraint("CK_GradeScale_Range", "\"MinScore\" >= 0 AND \"MaxScore\" <= 100 AND \"MinScore\" <= \"MaxScore\"");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Grade).HasMaxLength(15).IsRequired();
        e.HasIndex(x => new { x.EnterpriseId, x.Grade }).IsUnique();   // un nivel una sola vez por empresa
        e.HasOne<Enterprise>().WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Cascade);
    }
}

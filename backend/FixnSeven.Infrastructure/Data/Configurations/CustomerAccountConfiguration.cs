using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Infrastructure.Data.Configurations;

public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Email).HasMaxLength(254).IsRequired();
        e.HasIndex(x => x.Email).IsUnique();
        e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
        e.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        e.Property(x => x.Phone).HasMaxLength(30);
    }
}

public class CustomerDeviceConfiguration : IEntityTypeConfiguration<CustomerDevice>
{
    public void Configure(EntityTypeBuilder<CustomerDevice> e)
    {
        e.ToTable(t => t.HasCheckConstraint("CK_CustomerDevices_Status", "\"Status\" IN ('Activo','Transferido')"));
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Alias).HasMaxLength(60);
        e.Property(x => x.Accessories).HasMaxLength(300);
        e.Property(x => x.PublicDescription).HasMaxLength(500);
        e.Property(x => x.PrivateNotes).HasMaxLength(2000);
        e.Property(x => x.Status).HasMaxLength(15).IsRequired();
        e.Property(x => x.TransferCodeHash).HasMaxLength(100);

        e.HasIndex(x => x.CustomerAccountId);
        // Un equipo tiene como maximo UN dueno activo
        e.HasIndex(x => x.EquipmentId).IsUnique().HasFilter("\"Status\" = 'Activo'");

        e.HasOne<CustomerAccount>().WithMany().HasForeignKey(x => x.CustomerAccountId).OnDelete(DeleteBehavior.Cascade);
        e.HasOne<Equipment>().WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CustomerDeviceFileConfiguration : IEntityTypeConfiguration<CustomerDeviceFile>
{
    public void Configure(EntityTypeBuilder<CustomerDeviceFile> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_CustomerDeviceFiles_Kind", "\"FileKind\" IN ('Foto','PDF')");
            t.HasCheckConstraint("CK_CustomerDeviceFiles_PublicPhoto", "NOT \"IsPublic\" OR \"FileKind\" = 'Foto'");
        });
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        e.Property(x => x.FileKind).HasMaxLength(15).IsRequired();
        e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        e.Property(x => x.StorageKey).HasMaxLength(300).IsRequired();
        e.HasIndex(x => x.CustomerDeviceId);
        e.HasOne<CustomerDevice>().WithMany().HasForeignKey(x => x.CustomerDeviceId).OnDelete(DeleteBehavior.Cascade);
    }
}

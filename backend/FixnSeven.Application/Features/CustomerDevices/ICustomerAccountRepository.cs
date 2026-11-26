using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.CustomerDevices;

public record EquipmentInfo(string EquipmentType, string Brand, string Model);

// Todo aqui es GLOBAL (sin empresa): el filtro es la cuenta del comprador que sale del token
public interface ICustomerAccountRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task AddAccountAsync(CustomerAccount account);
    Task<CustomerAccount?> GetAccountByEmailAsync(string email);
    Task<CustomerAccount?> GetAccountAsync(Guid id);
    Task SaveChangesAsync();

    Task<Certificate?> GetCertificateByCodeAsync(string verificationCode);
    Task<CustomerDevice?> GetActiveDeviceByEquipmentAsync(Guid equipmentId);
    Task LinkAsync(CustomerDevice newDevice, CustomerDevice? previousOwner);   // en una transaccion

    Task<CustomerDevice?> GetMyDeviceAsync(Guid accountId, Guid deviceId);
    Task<(List<CustomerDevice> Items, int Total)> GetMyDevicesAsync(Guid accountId, int page, int pageSize);
    Task<EquipmentInfo> GetEquipmentInfoAsync(Guid equipmentId);
    Task<string?> GetLatestVerificationCodeAsync(Guid equipmentId);

    Task<int> CountFilesAsync(Guid deviceId);
    Task AddFileAsync(CustomerDeviceFile file);
    Task<List<CustomerDeviceFile>> GetFilesAsync(Guid deviceId);
    Task<CustomerDeviceFile?> GetFileAsync(Guid deviceId, Guid fileId);
    Task DeleteFileAsync(CustomerDeviceFile file);
    Task<CustomerDeviceFile?> GetPublicFileAsync(Guid fileId);   // foto publica de un dueno que comparte
}

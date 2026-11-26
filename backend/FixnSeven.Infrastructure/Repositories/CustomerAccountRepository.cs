using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.CustomerDevices;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
namespace FixnSeven.Infrastructure.Repositories;

// El comprador no tiene EnterpriseId: los datos de talleres se leen con IgnoreQueryFilters
// y SOLO los campos publicos. Sus propios equipos se filtran por CustomerAccountId.
public class CustomerAccountRepository : ICustomerAccountRepository
{
    private readonly FixnSevenDbContext _context;
    public CustomerAccountRepository(FixnSevenDbContext context) => _context = context;

    public Task<bool> EmailExistsAsync(string email) => _context.CustomerAccounts.AnyAsync(a => a.Email == email);

    public async Task AddAccountAsync(CustomerAccount account)
    {
        await _context.CustomerAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public Task<CustomerAccount?> GetAccountByEmailAsync(string email) =>
        _context.CustomerAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);

    public Task<CustomerAccount?> GetAccountAsync(Guid id) =>
        _context.CustomerAccounts.FirstOrDefaultAsync(a => a.Id == id);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    public Task<Certificate?> GetCertificateByCodeAsync(string verificationCode) =>
        _context.Certificates.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);

    public Task<CustomerDevice?> GetActiveDeviceByEquipmentAsync(Guid equipmentId) =>
        _context.CustomerDevices.FirstOrDefaultAsync(d => d.EquipmentId == equipmentId && d.Status == DeviceStatus.Active);

    // Primero se desvincula el dueno anterior (indice unico filtrado) y despues entra el nuevo
    public async Task LinkAsync(CustomerDevice newDevice, CustomerDevice? previousOwner)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.SaveChangesAsync();   // dueno anterior Transferido, codigo usado, intentos en 0
        await _context.CustomerDevices.AddAsync(newDevice);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public Task<CustomerDevice?> GetMyDeviceAsync(Guid accountId, Guid deviceId) =>
        _context.CustomerDevices.FirstOrDefaultAsync(d =>
            d.Id == deviceId && d.CustomerAccountId == accountId && d.Status == DeviceStatus.Active);

    public async Task<(List<CustomerDevice> Items, int Total)> GetMyDevicesAsync(Guid accountId, int page, int pageSize)
    {
        var query = _context.CustomerDevices.AsNoTracking()
            .Where(d => d.CustomerAccountId == accountId && d.Status == DeviceStatus.Active);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(d => d.LinkedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<EquipmentInfo> GetEquipmentInfoAsync(Guid equipmentId) =>
        await (from q in _context.Equipment.IgnoreQueryFilters()
               join c in _context.EquipmentCategories.IgnoreQueryFilters() on q.CategoryId equals c.Id
               where q.Id == equipmentId
               select new EquipmentInfo(c.Name, q.Brand, q.Model)).FirstAsync();

    // El certificado mas reciente que no este revocado; si todos lo estan, el ultimo
    public async Task<string?> GetLatestVerificationCodeAsync(Guid equipmentId) =>
        await _context.Certificates.IgnoreQueryFilters()
            .Where(c => c.EquipmentId == equipmentId)
            .OrderBy(c => c.Status == CertificateStatus.Revoked)
            .ThenByDescending(c => c.IssuedAt)
            .Select(c => c.VerificationCode)
            .FirstOrDefaultAsync();

    public Task<int> CountFilesAsync(Guid deviceId) =>
        _context.CustomerDeviceFiles.CountAsync(f => f.CustomerDeviceId == deviceId);

    public async Task AddFileAsync(CustomerDeviceFile file)
    {
        await _context.CustomerDeviceFiles.AddAsync(file);
        await _context.SaveChangesAsync();
    }

    public Task<List<CustomerDeviceFile>> GetFilesAsync(Guid deviceId) =>
        _context.CustomerDeviceFiles.AsNoTracking()
            .Where(f => f.CustomerDeviceId == deviceId).OrderBy(f => f.UploadedAt).ToListAsync();

    public Task<CustomerDeviceFile?> GetFileAsync(Guid deviceId, Guid fileId) =>
        _context.CustomerDeviceFiles.FirstOrDefaultAsync(f => f.CustomerDeviceId == deviceId && f.Id == fileId);

    public async Task DeleteFileAsync(CustomerDeviceFile file)
    {
        _context.CustomerDeviceFiles.Remove(file);
        await _context.SaveChangesAsync();
    }

    public Task<CustomerDeviceFile?> GetPublicFileAsync(Guid fileId) =>
        (from f in _context.CustomerDeviceFiles
         join d in _context.CustomerDevices on f.CustomerDeviceId equals d.Id
         where f.Id == fileId && f.IsPublic && f.FileKind == FileKinds.Photo
               && d.Status == DeviceStatus.Active && d.ShowInPublicView
         select f).AsNoTracking().FirstOrDefaultAsync();
}

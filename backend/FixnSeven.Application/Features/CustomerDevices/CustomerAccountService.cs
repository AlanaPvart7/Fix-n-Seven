using FixnSeven.Application.Common;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
using FixnSeven.Application.Features.Reports;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.CustomerDevices;

// Fase 2: cuenta del comprador final (reglas 14 y 16 del modelo)
public class CustomerAccountService
{
    private const int MaxFailedLinks = 5;
    private static readonly TimeSpan LinkLock = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan TransferValidity = TimeSpan.FromDays(7);
    private const int MaxFilesPerDevice = 10;

    private readonly ICustomerAccountRepository _repository;
    private readonly ICertificateRepository _certificates;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IFileStorage _storage;
    private readonly ICurrentTenant _tenant;

    public CustomerAccountService(ICustomerAccountRepository repository, ICertificateRepository certificates,
        IPasswordHasher hasher, ITokenService tokens, IFileStorage storage, ICurrentTenant tenant)
    {
        _repository = repository;
        _certificates = certificates;
        _hasher = hasher;
        _tokens = tokens;
        _storage = storage;
        _tenant = tenant;
    }

    private Guid Me => _tenant.CustomerAccountId ?? throw new InvalidOperationException("No hay cuenta de comprador en el token.");

    // ---------- Registro y login ----------
    public async Task<ServiceResult<CustomerLoginResponseDto>> RegisterAsync(RegisterCustomerRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email))
            return ServiceResult<CustomerLoginResponseDto>.Fail(409, "Ese correo ya tiene una cuenta.");

        var account = new CustomerAccount
        {
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim()
        };
        await _repository.AddAccountAsync(account);

        var (token, expiresAt) = _tokens.CreateCustomerToken(account);
        return ServiceResult<CustomerLoginResponseDto>.Ok(
            new CustomerLoginResponseDto(token, expiresAt, account.Id, account.FullName), 201);
    }

    public async Task<ServiceResult<CustomerLoginResponseDto>> LoginAsync(CustomerLoginRequestDto request)
    {
        var account = await _repository.GetAccountByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (account is null || !_hasher.Verify(request.Password, account.PasswordHash))
            return ServiceResult<CustomerLoginResponseDto>.Fail(401, "Correo o contraseña incorrectos.");

        var (token, expiresAt) = _tokens.CreateCustomerToken(account);
        return ServiceResult<CustomerLoginResponseDto>.Ok(new CustomerLoginResponseDto(token, expiresAt, account.Id, account.FullName));
    }

    // ---------- Vincular un equipo ----------
    // Con el codigo privado del dueno (un solo uso) o con un codigo de transferencia vigente.
    // Asi nadie se apropia de un equipo solo con escanear su QR publico.
    public async Task<ServiceResult<CustomerDeviceResponseDto>> LinkAsync(LinkDeviceRequestDto request)
    {
        var account = await _repository.GetAccountAsync(Me);
        if (account is null) return ServiceResult<CustomerDeviceResponseDto>.Fail(401, "La cuenta no existe.");

        var now = DateTime.UtcNow;
        if (account.LinkLockedUntil > now)
        {
            var minutes = (int)Math.Ceiling((account.LinkLockedUntil.Value - now).TotalMinutes);
            return ServiceResult<CustomerDeviceResponseDto>.Fail(429,
                $"Demasiados intentos fallidos. Intente de nuevo en {minutes} minutos.");
        }

        var certificate = await _repository.GetCertificateByCodeAsync(request.VerificationCode.Trim().ToUpperInvariant());
        var code = request.Code.Trim().ToUpperInvariant();
        var active = certificate is null ? null : await _repository.GetActiveDeviceByEquipmentAsync(certificate.EquipmentId);

        var isOwnerCode = certificate is not null && certificate.OwnerCodeUsedAt is null
                          && _hasher.Verify(code, certificate.OwnerCodeHash);
        var isTransferCode = !isOwnerCode && active?.TransferCodeHash is not null
                             && active.TransferExpiresAt > now && _hasher.Verify(code, active.TransferCodeHash);

        if (!isOwnerCode && !isTransferCode)
        {
            account.FailedLinkAttempts++;
            if (account.FailedLinkAttempts >= MaxFailedLinks)
            {
                account.FailedLinkAttempts = 0;
                account.LinkLockedUntil = now.Add(LinkLock);
            }
            await _repository.SaveChangesAsync();
            // Mismo mensaje para todo: no se revela que parte esta mal
            return ServiceResult<CustomerDeviceResponseDto>.Fail(400, "El código de verificación o el código del dueño no son válidos.");
        }

        account.FailedLinkAttempts = 0;
        account.LinkLockedUntil = null;
        CustomerDevice? previousOwner = null;

        if (isTransferCode)
        {
            if (active!.CustomerAccountId == account.Id)
                return ServiceResult<CustomerDeviceResponseDto>.Fail(400, "Ese equipo ya está en su cuenta.");
            // La informacion del dueno anterior se desvincula del equipo
            active.Status = DeviceStatus.Transferred;
            active.TransferCodeHash = null;
            active.TransferExpiresAt = null;
            previousOwner = active;
        }
        else
        {
            if (active is not null)
                return ServiceResult<CustomerDeviceResponseDto>.Fail(409,
                    "El equipo ya tiene un dueño. Pídale un código de transferencia.");
            certificate!.OwnerCodeUsedAt = now;   // el codigo del dueno ya no sirve otra vez
        }

        var device = new CustomerDevice { CustomerAccountId = account.Id, EquipmentId = certificate!.EquipmentId };
        await _repository.LinkAsync(device, previousOwner);
        return ServiceResult<CustomerDeviceResponseDto>.Ok(await ToDtoAsync(device), 201);
    }

    // ---------- Mis equipos ----------
    public async Task<(List<CustomerDeviceResponseDto> Items, int Total)> GetMyDevicesAsync(int page, int pageSize)
    {
        var (devices, total) = await _repository.GetMyDevicesAsync(Me, page, pageSize);
        var items = new List<CustomerDeviceResponseDto>();
        foreach (var d in devices) items.Add(await ToDtoAsync(d));
        return (items, total);
    }

    public async Task<CustomerDeviceResponseDto?> GetMyDeviceAsync(Guid id)
    {
        var device = await _repository.GetMyDeviceAsync(Me, id);
        return device is null ? null : await ToDtoAsync(device);
    }

    // PUT: devuelve el equipo con advertencias (excepcion del contrato: 200 en lugar de 204)
    public async Task<ServiceResult<CustomerDeviceResponseDto>> UpdateAsync(Guid id, UpdateCustomerDeviceRequestDto request)
    {
        var device = await _repository.GetMyDeviceAsync(Me, id);
        if (device is null) return ServiceResult<CustomerDeviceResponseDto>.Fail(404, "El equipo no existe en su cuenta.");

        device.Alias = Clean(request.Alias);
        device.PurchaseDate = request.PurchaseDate;
        device.Accessories = Clean(request.Accessories);
        device.PublicDescription = Clean(request.PublicDescription);
        device.PrivateNotes = Clean(request.PrivateNotes);
        device.ShowInPublicView = request.ShowInPublicView;
        await _repository.SaveChangesAsync();

        List<string>? warnings = request.ShowInPublicView
            ? ["Su información será visible en la consulta pública del QR. No incluya nombre, teléfono, correo ni dirección en la descripción ni en las fotos."]
            : null;
        return ServiceResult<CustomerDeviceResponseDto>.Ok(await ToDtoAsync(device, warnings));
    }

    // ---------- Archivos (fotos y PDF) ----------
    public async Task<ServiceResult<CustomerDeviceFileResponseDto>> AddFileAsync(
        Guid id, string fileName, Stream content, bool isPublic)
    {
        var device = await _repository.GetMyDeviceAsync(Me, id);
        if (device is null) return ServiceResult<CustomerDeviceFileResponseDto>.Fail(404, "El equipo no existe en su cuenta.");
        if (await _repository.CountFilesAsync(id) >= MaxFilesPerDevice)
            return ServiceResult<CustomerDeviceFileResponseDto>.Fail(409, $"Un equipo admite como máximo {MaxFilesPerDevice} archivos.");

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        if (bytes.Length == 0) return ServiceResult<CustomerDeviceFileResponseDto>.Fail(400, "El archivo está vacío.");

        var kind = FileInspector.Inspect(bytes.Take(1024).ToArray(), fileName);
        if (kind is null || kind.FileKind is not (FileKinds.Photo or FileKinds.Pdf))
            return ServiceResult<CustomerDeviceFileResponseDto>.Fail(400,
                "Solo se aceptan fotos (JPG, PNG, WEBP, HEIC) y PDF, y el tipo real debe coincidir con la extensión.");
        if (bytes.LongLength > kind.MaxBytes)
            return ServiceResult<CustomerDeviceFileResponseDto>.Fail(400, "El archivo pasa de 10 MB.");
        if (isPublic && kind.FileKind != FileKinds.Photo)
            return ServiceResult<CustomerDeviceFileResponseDto>.Fail(400, "Solo las fotos pueden ser públicas; los PDF son siempre privados.");

        var file = new CustomerDeviceFile
        {
            CustomerDeviceId = device.Id,
            FileName = Path.GetFileName(fileName),
            FileKind = kind.FileKind,
            ContentType = kind.ContentType,
            SizeBytes = bytes.LongLength,
            IsPublic = isPublic
        };
        file.StorageKey = $"devices/{device.Id}/{file.Id}{Path.GetExtension(fileName).ToLowerInvariant()}";

        buffer.Position = 0;
        await _storage.SaveAsync(file.StorageKey, buffer);
        await _repository.AddFileAsync(file);
        return ServiceResult<CustomerDeviceFileResponseDto>.Ok(ToFileDto(file), 201);
    }

    public async Task<bool> DeleteFileAsync(Guid id, Guid fileId)
    {
        if (await _repository.GetMyDeviceAsync(Me, id) is null) return false;
        var file = await _repository.GetFileAsync(id, fileId);
        if (file is null) return false;
        await _storage.DeleteAsync(file.StorageKey);
        await _repository.DeleteFileAsync(file);
        return true;
    }

    // Publico: foto que el dueno decidio compartir en la consulta del QR
    public async Task<(Stream Content, string ContentType)?> GetPublicPhotoAsync(Guid fileId)
    {
        var file = await _repository.GetPublicFileAsync(fileId);
        if (file is null) return null;
        var stream = await _storage.OpenReadAsync(file.StorageKey);
        return stream is null ? null : (stream, file.ContentType);
    }

    // ---------- Transferir a un nuevo dueno ----------
    public async Task<ServiceResult<TransferCodeResponseDto>> CreateTransferCodeAsync(Guid id)
    {
        var device = await _repository.GetMyDeviceAsync(Me, id);
        if (device is null) return ServiceResult<TransferCodeResponseDto>.Fail(404, "El equipo no existe en su cuenta.");

        var code = CodeGenerator.CreateGrouped(3, 4);   // se muestra UNA vez; solo se guarda la huella
        device.TransferCodeHash = _hasher.Hash(code);
        device.TransferExpiresAt = DateTime.UtcNow.Add(TransferValidity);
        await _repository.SaveChangesAsync();
        return ServiceResult<TransferCodeResponseDto>.Ok(new TransferCodeResponseDto(code, device.TransferExpiresAt.Value), 201);
    }

    // ---------- Mapeo ----------
    private async Task<CustomerDeviceResponseDto> ToDtoAsync(CustomerDevice d, List<string>? warnings = null)
    {
        var info = await _repository.GetEquipmentInfoAsync(d.EquipmentId);
        var code = await _repository.GetLatestVerificationCodeAsync(d.EquipmentId);
        var certificate = code is null ? null : await _certificates.GetPublicAsync(code);
        var files = (await _repository.GetFilesAsync(d.Id)).Select(ToFileDto).ToList();
        return new CustomerDeviceResponseDto(d.Id, d.Alias, info.EquipmentType, info.Brand, info.Model, certificate,
            d.PurchaseDate, d.Accessories, d.PublicDescription, d.PrivateNotes, d.ShowInPublicView, files, warnings);
    }

    private static CustomerDeviceFileResponseDto ToFileDto(CustomerDeviceFile f) =>
        new(f.Id, f.FileName, f.FileKind, f.ContentType, f.SizeBytes, f.IsPublic);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

namespace FixnSeven.Application.Features.Reports;

// Almacenamiento PRIVADO de archivos (local en desarrollo; S3 o similar en produccion)
public interface IFileStorage
{
    Task SaveAsync(string key, Stream content);
    Task<Stream?> OpenReadAsync(string key);
    Task DeleteAsync(string key);
}

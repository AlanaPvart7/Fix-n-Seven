using FixnSeven.Application.Features.Reports;
namespace FixnSeven.Infrastructure.Storage;

// Almacenamiento privado en una carpeta del servidor (fuera de wwwroot: no se puede abrir por URL).
// En produccion se cambia por S3, Cloudflare R2 o similar, sin tocar Application.
public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(string root)
    {
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    private string PathFor(string key)
    {
        var full = Path.GetFullPath(Path.Combine(_root, key));
        if (!full.StartsWith(_root, StringComparison.Ordinal))
            throw new InvalidOperationException("Ruta de archivo no valida.");   // evita salir de la carpeta
        return full;
    }

    public async Task SaveAsync(string key, Stream content)
    {
        var path = PathFor(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file);
    }

    public Task<Stream?> OpenReadAsync(string key)
    {
        var path = PathFor(key);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(string key)
    {
        var path = PathFor(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}

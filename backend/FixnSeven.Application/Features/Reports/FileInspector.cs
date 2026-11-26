using System.Text;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Reports;

// Revisa el tipo REAL del archivo por sus primeros bytes, no solo por la extension.
// Rechaza ejecutables, comprimidos y todo lo que no este en la lista.
public static class FileInspector
{
    public const int MaxFilesPerReport = 5;
    private const long TenMb = 10 * 1024 * 1024;
    private const long FiftyMb = 50 * 1024 * 1024;

    public record Result(string FileKind, string ContentType, long MaxBytes);

    public static Result? Inspect(byte[] head, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        bool StartsWith(params byte[] sig) => head.Length >= sig.Length && head.AsSpan(0, sig.Length).SequenceEqual(sig);
        string Ascii(int start, int length) =>
            head.Length >= start + length ? Encoding.ASCII.GetString(head, start, length) : string.Empty;

        // Fotos
        if (StartsWith(0xFF, 0xD8, 0xFF) && ext is ".jpg" or ".jpeg") return new(FileKinds.Photo, "image/jpeg", TenMb);
        if (StartsWith(0x89, 0x50, 0x4E, 0x47) && ext == ".png") return new(FileKinds.Photo, "image/png", TenMb);
        if (Ascii(0, 4) == "RIFF" && Ascii(8, 4) == "WEBP" && ext == ".webp") return new(FileKinds.Photo, "image/webp", TenMb);
        var brand = Ascii(4, 8);   // "ftypXXXX" en HEIC, MP4 y MOV
        if (brand is "ftypheic" or "ftypheix" or "ftyphevc" or "ftypmif1" && ext == ".heic")
            return new(FileKinds.Photo, "image/heic", TenMb);

        // PDF
        if (Ascii(0, 5) == "%PDF-" && ext == ".pdf") return new(FileKinds.Pdf, "application/pdf", TenMb);

        // Videos
        if (brand.StartsWith("ftyp") && ext == ".mp4") return new(FileKinds.Video, "video/mp4", FiftyMb);
        if (brand.StartsWith("ftyp") && ext == ".mov") return new(FileKinds.Video, "video/quicktime", FiftyMb);

        // Documentos de Office (son ZIP por dentro; un .zip suelto se rechaza)
        if (StartsWith(0x50, 0x4B, 0x03, 0x04) && ext == ".docx")
            return new(FileKinds.Document, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", TenMb);
        if (StartsWith(0x50, 0x4B, 0x03, 0x04) && ext == ".xlsx")
            return new(FileKinds.Document, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", TenMb);

        // Texto plano: sin bytes nulos ni cabecera de ejecutable
        if (ext == ".txt" && !StartsWith(0x4D, 0x5A) && !head.Contains((byte)0))
            return new(FileKinds.Document, "text/plain", TenMb);

        return null;
    }
}

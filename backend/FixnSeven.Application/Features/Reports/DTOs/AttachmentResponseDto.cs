namespace FixnSeven.Application.Features.Reports.DTOs;

public record AttachmentResponseDto(
    Guid Id,
    string FileName,
    string FileKind,   // Foto, PDF, Video o Documento
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt
);

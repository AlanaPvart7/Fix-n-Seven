namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

public record CustomerDeviceFileResponseDto(
    Guid Id,
    string FileName,
    string FileKind,   // Foto o PDF
    string ContentType,
    long SizeBytes,
    bool IsPublic
);

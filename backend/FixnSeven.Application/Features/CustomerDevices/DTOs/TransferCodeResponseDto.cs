namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

// Se muestra UNA sola vez: el comprador se lo da al nuevo dueno
public record TransferCodeResponseDto(
    string TransferCode,
    DateTime ExpiresAt
);

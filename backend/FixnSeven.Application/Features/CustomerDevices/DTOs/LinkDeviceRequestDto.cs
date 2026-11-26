namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

public record LinkDeviceRequestDto(
    string VerificationCode,  // el del QR
    string Code               // codigo privado del dueno (PDF de la orden) o codigo de transferencia
);

namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

public record CustomerLoginResponseDto(
    string Token,
    DateTime ExpiresAt,
    Guid CustomerAccountId,
    string FullName
);

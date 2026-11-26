namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

public record RegisterCustomerRequestDto(
    string Email,
    string Password,
    string FullName,
    string? Phone
);

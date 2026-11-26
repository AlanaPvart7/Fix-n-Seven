namespace FixnSeven.Application.Features.CustomerDevices.DTOs;

public record UpdateCustomerDeviceRequestDto(
    string? Alias,
    DateOnly? PurchaseDate,
    string? Accessories,
    string? PublicDescription,
    string? PrivateNotes,
    bool ShowInPublicView
);

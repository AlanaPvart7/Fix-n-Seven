namespace FixnSeven.Application.Features.Customers.DTOs;

public record UpdateCustomerRequestDto(string FullName, string Phone, string? Email);
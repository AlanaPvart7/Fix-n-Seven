namespace FixnSeven.Application.Features.Customers.DTOs;

// Lo que la API devuelve. Sin EnterpriseId: el frontend no lo necesita.
public record CustomerResponseDto(Guid Id, string FullName, string Phone, string? Email);
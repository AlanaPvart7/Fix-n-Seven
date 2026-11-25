namespace FixnSeven.Application.Features.Customers.DTOs;

// Lo que el cliente (React) PUEDE enviar. Sin Id ni EnterpriseId: así no hay over-posting.
public record CreateCustomerRequestDto(string FullName, string Phone, string? Email);
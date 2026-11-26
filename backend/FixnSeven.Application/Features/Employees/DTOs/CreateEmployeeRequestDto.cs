namespace FixnSeven.Application.Features.Employees.DTOs;

public record CreateEmployeeRequestDto(
    string FullName,
    string Email,
    string Password,
    string Role          // Admin, Reception o Technician
);

namespace FixnSeven.Application.Features.Employees.DTOs;

public record UpdateEmployeeRequestDto(
    string FullName,
    string Role          // Admin, Reception o Technician
);

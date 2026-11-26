namespace FixnSeven.Application.Features.Employees.DTOs;

public record EmployeeResponseDto(Guid Id, string FullName, string Email, string Role);

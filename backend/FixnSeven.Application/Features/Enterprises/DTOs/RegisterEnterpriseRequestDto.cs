namespace FixnSeven.Application.Features.Enterprises.DTOs;

public record RegisterEnterpriseRequestDto(
    string EnterpriseName,
    Guid EnterpriseTypeId,
    Guid PlanId,
    string AdminFullName,
    string AdminEmail,
    string Password
);

namespace FixnSeven.Application.Features.Auth.DTOs;

public record LoginResponseDto(string Token, DateTime ExpiresAt, string Role, Guid EnterpriseId);

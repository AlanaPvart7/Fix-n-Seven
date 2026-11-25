using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Employee employee, string roleName);
}

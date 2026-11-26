using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Employee employee, string roleName);
    (string Token, DateTime ExpiresAt) CreateCustomerToken(CustomerAccount account);   // rol Customer, sin empresa
}

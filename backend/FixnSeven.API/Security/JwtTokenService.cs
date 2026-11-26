using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Domain.Entities;
namespace FixnSeven.API.Security;

public class JwtTokenService : ITokenService
{
    private readonly string _key;
    private readonly string _issuer;

    public JwtTokenService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt__Key en el .env");
        _issuer = configuration["Jwt:Issuer"] ?? "FixnSeven";
    }

    public (string Token, DateTime ExpiresAt) CreateToken(Employee employee, string roleName)
    {
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim("employee_id", employee.Id.ToString()),
            new Claim("enterprise_id", employee.EnterpriseId.ToString()),   // el tenant viaja en el token
            new Claim(ClaimTypes.Role, roleName),                           // Admin, Reception o Technician
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim(ClaimTypes.Name, employee.FullName)
        };

        return (Write(claims, expiresAt), expiresAt);
    }

    // Comprador final: NO lleva enterprise_id, asi los filtros globales no le dejan ver datos de talleres
    public (string Token, DateTime ExpiresAt) CreateCustomerToken(CustomerAccount account)
    {
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var claims = new[]
        {
            new Claim("customer_account_id", account.Id.ToString()),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Name, account.FullName)
        };
        return (Write(claims, expiresAt), expiresAt);
    }

    private string Write(Claim[] claims, DateTime expiresAt)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_issuer, _issuer, claims, expires: expiresAt, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

namespace FixnSeven.Application.Common.Interfaces;

// Quien hace la peticion: sale del token JWT, NUNCA del body (HU-04)
public interface ICurrentTenant
{
    Guid? EnterpriseId { get; }
    Guid? EmployeeId { get; }
    string? Role { get; }          // Admin, Reception, Technician o Customer
    Guid? CustomerAccountId { get; }   // solo en tokens de compradores (rol Customer)
}

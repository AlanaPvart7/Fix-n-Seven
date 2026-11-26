using FixnSeven.Application.Common.Interfaces;
namespace FixnSeven.API.Security;

public class JwtCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _accessor;
    public JwtCurrentTenant(IHttpContextAccessor accessor) => _accessor = accessor;

    private Guid? ReadClaim(string type) =>
        Guid.TryParse(_accessor.HttpContext?.User.FindFirst(type)?.Value, out var id) ? id : null;

    public Guid? EnterpriseId => ReadClaim("enterprise_id");
    public Guid? EmployeeId => ReadClaim("employee_id");
}

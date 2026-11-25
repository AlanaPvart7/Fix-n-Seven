using FixnSeven.Application.Features.Auth.DTOs;
namespace FixnSeven.Application.Features.Auth;

// Resultado de una operacion de autenticacion: datos + codigo HTTP sugerido + error
public record AuthResult(LoginResponseDto? Data, int StatusCode, string? Error)
{
    public static AuthResult Ok(LoginResponseDto data, int statusCode = 200) => new(data, statusCode, null);
    public static AuthResult Fail(int statusCode, string error) => new(null, statusCode, error);
}

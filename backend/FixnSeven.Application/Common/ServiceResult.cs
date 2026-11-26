namespace FixnSeven.Application.Common;

// Resultado de un servicio: datos + codigo HTTP sugerido + mensaje de error
public record ServiceResult<T>(T? Data, int StatusCode, string? Error)
{
    public bool IsSuccess => Error is null;
    public static ServiceResult<T> Ok(T data, int statusCode = 200) => new(data, statusCode, null);
    public static ServiceResult<T> Fail(int statusCode, string error) => new(default, statusCode, error);
}

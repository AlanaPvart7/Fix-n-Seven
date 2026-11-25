using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Auth;
using FixnSeven.Application.Features.Auth.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IValidator<LoginRequestDto> _validator;

    public AuthController(AuthService authService, IValidator<LoginRequestDto> validator)
    {
        _authService = authService;
        _validator = validator;
    }

    // POST /api/auth/login  (HU-02)
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid) return BadRequest(validation.Errors);

        var result = await _authService.LoginAsync(request);
        return result.Data is not null
            ? Ok(result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

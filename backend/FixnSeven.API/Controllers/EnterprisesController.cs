using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Auth;
using FixnSeven.Application.Features.Auth.DTOs;
using FixnSeven.Application.Features.Enterprises.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/enterprises")]
public class EnterprisesController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IValidator<RegisterEnterpriseRequestDto> _validator;

    public EnterprisesController(AuthService authService, IValidator<RegisterEnterpriseRequestDto> validator)
    {
        _authService = authService;
        _validator = validator;
    }

    // POST /api/enterprises/register  (HU-01)
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterEnterpriseRequestDto request)
    {
        await _validator.ValidateAndThrowAsync(request);   // si falla, el middleware responde 400

        var result = await _authService.RegisterEnterpriseAsync(request);
        return result.Data is not null
            ? StatusCode(StatusCodes.Status201Created, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Auth;
using FixnSeven.Application.Features.Auth.DTOs;
using FixnSeven.Application.Features.Enterprises;
using FixnSeven.Application.Features.Enterprises.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/enterprises")]
public class EnterprisesController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IValidator<RegisterEnterpriseRequestDto> _validator;
    private readonly ICatalogRepository _catalogs;
    private readonly ICurrentTenant _tenant;

    public EnterprisesController(
        AuthService authService,
        IValidator<RegisterEnterpriseRequestDto> validator,
        ICatalogRepository catalogs,
        ICurrentTenant tenant)
    {
        _authService = authService;
        _validator = validator;
        _catalogs = catalogs;
        _tenant = tenant;
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
            ? CreatedAtAction(nameof(GetMyEnterprise), null, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // GET /api/enterprises/me  (datos de la empresa del token)
    [Authorize(Roles = "Admin")]
    [HttpGet("me")]
    [ProducesResponseType<EnterpriseResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyEnterprise()
    {
        var enterprise = await _catalogs.GetEnterpriseAsync(_tenant.EnterpriseId!.Value);
        return enterprise is null ? NotFound() : Ok(enterprise);
    }
}

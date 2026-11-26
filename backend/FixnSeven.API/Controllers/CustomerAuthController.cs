using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.CustomerDevices;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
namespace FixnSeven.API.Controllers;

// Fase 2: cuentas GLOBALES de compradores (rol Customer, sin empresa)
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class CustomerAuthController : ControllerBase
{
    private readonly CustomerAccountService _service;
    private readonly IValidator<RegisterCustomerRequestDto> _registerValidator;
    private readonly IValidator<CustomerLoginRequestDto> _loginValidator;

    public CustomerAuthController(CustomerAccountService service,
        IValidator<RegisterCustomerRequestDto> registerValidator, IValidator<CustomerLoginRequestDto> loginValidator)
    {
        _service = service;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    // POST /api/auth/customer-register
    [HttpPost("customer-register")]
    [ProducesResponseType<CustomerLoginResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequestDto request)
    {
        await _registerValidator.ValidateAndThrowAsync(request);
        var result = await _service.RegisterAsync(request);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // POST /api/auth/customer-login
    [HttpPost("customer-login")]
    [ProducesResponseType<CustomerLoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] CustomerLoginRequestDto request)
    {
        await _loginValidator.ValidateAndThrowAsync(request);
        var result = await _service.LoginAsync(request);
        return result.IsSuccess ? Ok(result.Data) : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

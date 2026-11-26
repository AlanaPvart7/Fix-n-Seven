using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Settings;
using FixnSeven.Application.Features.Settings.DTOs;
namespace FixnSeven.API.Controllers;

// Fase 2: vigencia del certificado y escala de calidad de MI empresa
[ApiController]
[Route("api/certificate-settings")]
[Authorize]
public class CertificateSettingsController : ControllerBase
{
    private readonly SettingsService _service;
    private readonly IValidator<UpdateCertificateSettingsRequestDto> _validator;

    public CertificateSettingsController(SettingsService service, IValidator<UpdateCertificateSettingsRequestDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    // GET /api/certificate-settings  (todos: el tecnico la necesita para emitir)
    [Authorize(Roles = "Admin,Reception,Technician")]
    [HttpGet]
    [ProducesResponseType<CertificateSettingsResponseDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings() => Ok(await _service.GetAsync());

    // PUT /api/certificate-settings  (solo Admin)
    [Authorize(Roles = "Admin")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateCertificateSettingsRequestDto request)
    {
        await _validator.ValidateAndThrowAsync(request);
        await _service.UpdateAsync(request);
        return NoContent();
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Certificates.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/certificates")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly CertificateService _service;
    private readonly IValidator<IssueCertificateRequestDto> _issueValidator;
    private readonly IValidator<RevokeCertificateRequestDto> _revokeValidator;

    public CertificatesController(CertificateService service,
        IValidator<IssueCertificateRequestDto> issueValidator,
        IValidator<RevokeCertificateRequestDto> revokeValidator)
    {
        _service = service;
        _issueValidator = issueValidator;
        _revokeValidator = revokeValidator;
    }

    // GET /api/certificates/{id}
    [Authorize(Roles = "Admin,Reception,Technician")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CertificateResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCertificate(Guid id)
    {
        var certificate = await _service.GetByIdAsync(id);
        return certificate is null ? NotFound() : Ok(certificate);
    }

    // POST /api/certificates  (HU-14) — el OwnerCode viene SOLO en esta respuesta
    [Authorize(Roles = "Admin,Technician")]
    [HttpPost]
    [ProducesResponseType<CertificateResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IssueCertificate([FromBody] IssueCertificateRequestDto request)
    {
        await _issueValidator.ValidateAndThrowAsync(request);
        var result = await _service.IssueAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCertificate), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // PUT /api/certificates/{id}/revoke  (HU-15)
    [Authorize(Roles = "Admin,Technician")]
    [HttpPut("{id:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevokeCertificate(Guid id, [FromBody] RevokeCertificateRequestDto request)
    {
        await _revokeValidator.ValidateAndThrowAsync(request);
        var result = await _service.RevokeAsync(id, request);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

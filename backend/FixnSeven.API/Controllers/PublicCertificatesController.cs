using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Certificates.DTOs;
namespace FixnSeven.API.Controllers;

// HU-16: lo que ve cualquiera que escanea el QR, SIN cuenta y SIN datos personales
[ApiController]
[Route("api/public/certificates")]
[AllowAnonymous]
public class PublicCertificatesController : ControllerBase
{
    private readonly CertificateService _service;
    public PublicCertificatesController(CertificateService service) => _service = service;

    // GET /api/public/certificates/{code}
    [HttpGet("{code}")]
    [ProducesResponseType<PublicCertificateResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicCertificate(string code)
    {
        var certificate = await _service.GetPublicAsync(code);
        return certificate is null ? NotFound() : Ok(certificate);
    }
}

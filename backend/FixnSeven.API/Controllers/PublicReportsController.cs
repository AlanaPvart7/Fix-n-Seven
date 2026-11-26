using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Reports;
using FixnSeven.Application.Features.Reports.DTOs;
namespace FixnSeven.API.Controllers;

// Fase 2: cualquiera reporta un certificado desde el enlace del QR, SIN cuenta.
// Si manda token de su empresa, queda como Distribuidor (sesion opcional).
[ApiController]
[AllowAnonymous]
public class PublicReportsController : ControllerBase
{
    private const long MaxUploadBytes = 52 * 1024 * 1024;   // video de 50 MB + margen del formulario

    private readonly ReportService _service;
    private readonly IValidator<CreateReportRequestDto> _validator;

    public PublicReportsController(ReportService service, IValidator<CreateReportRequestDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    // POST /api/public/certificates/{code}/reports
    [HttpPost("api/public/certificates/{code}/reports")]
    [ProducesResponseType<ReportResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateReport(string code, [FromBody] CreateReportRequestDto request)
    {
        await _validator.ValidateAndThrowAsync(request);
        var result = await _service.CreateAsync(code, request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetReportStatus), new { trackingCode = result.Data!.TrackingCode }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // POST /api/public/reports/{trackingCode}/attachments  (multipart/form-data, campo "file")
    [HttpPost("api/public/reports/{trackingCode}/attachments")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<AttachmentResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAttachment(string trackingCode, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var result = await _service.AddAttachmentAsync(trackingCode, file.FileName, file.Length, stream);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetReportStatus), new { trackingCode }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // GET /api/public/reports/{trackingCode}
    [HttpGet("api/public/reports/{trackingCode}")]
    [ProducesResponseType<PublicReportStatusResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportStatus(string trackingCode)
    {
        var status = await _service.GetPublicStatusAsync(trackingCode);
        return status is null ? NotFound() : Ok(status);
    }
}

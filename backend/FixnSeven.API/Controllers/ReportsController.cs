using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.Reports;
using FixnSeven.Application.Features.Reports.DTOs;
namespace FixnSeven.API.Controllers;

// Fase 2: la empresa que emitio el certificado revisa y resuelve sus reportes
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _service;
    private readonly IValidator<ResolveReportRequestDto> _resolveValidator;

    public ReportsController(ReportService service, IValidator<ResolveReportRequestDto> resolveValidator)
    {
        _service = service;
        _resolveValidator = resolveValidator;
    }

    // GET /api/reports?status=&certificateId=&page=1&pageSize=20
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<ReportSummaryResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports([FromQuery] string? status, [FromQuery] Guid? certificateId,
        int page = 1, int pageSize = 20)
    {
        var (items, total) = await _service.GetPagedAsync(status, certificateId, page, pageSize);
        return Ok(new PagedResponseDto<ReportSummaryResponseDto>(items, total, page, pageSize));
    }

    // GET /api/reports/{id}  (el tecnico, solo de sus certificados)
    [Authorize(Roles = "Admin,Technician")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ReportDetailResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(Guid id)
    {
        var report = await _service.GetDetailAsync(id);
        return report is null ? NotFound() : Ok(report);
    }

    // GET /api/reports/{id}/attachments/{attachmentId}  (descarga la prueba)
    [Authorize(Roles = "Admin,Technician")]
    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId)
    {
        var file = await _service.DownloadAsync(id, attachmentId);
        return file is null ? NotFound() : File(file.Value.Content, file.Value.ContentType, file.Value.FileName);
    }

    // PUT /api/reports/{id}/review  (pasa a En revision)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReviewReport(Guid id)
    {
        var result = await _service.ReviewAsync(id);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // PUT /api/reports/{id}/resolve  (Aceptado o Rechazado, con explicacion)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequestDto request)
    {
        await _resolveValidator.ValidateAndThrowAsync(request);
        var result = await _service.ResolveAsync(id, request);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

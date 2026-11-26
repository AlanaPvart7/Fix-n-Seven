using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Income;
using FixnSeven.Application.Features.Income.DTOs;
namespace FixnSeven.API.Controllers;

// HU-21 (Fase 2): solo el dueno (Admin) ve cuanto facturo
[ApiController]
[Route("api/income-reports")]
[Authorize(Roles = "Admin")]
public class IncomeReportsController : ControllerBase
{
    private readonly IncomeService _service;
    public IncomeReportsController(IncomeService service) => _service = service;

    // GET /api/income-reports?from=2026-11-01&to=2026-11-30&groupBy=day
    [HttpGet]
    [ProducesResponseType<IncomeReportResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetIncomeReport([FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] string? groupBy)
    {
        var result = await _service.GetReportAsync(from, to, groupBy);
        return result.IsSuccess ? Ok(result.Data) : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

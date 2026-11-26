using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Reminders;
using FixnSeven.Application.Features.Reminders.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/reminders")]
[Authorize(Roles = "Admin,Reception")]
public class RemindersController : ControllerBase
{
    private readonly ReminderService _service;
    private readonly IValidator<CreateReminderRequestDto> _createValidator;

    public RemindersController(ReminderService service, IValidator<CreateReminderRequestDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    // GET /api/reminders/upcoming?days=30  (HU-17: vencidos primero, luego los mas cercanos)
    [HttpGet("upcoming")]
    [ProducesResponseType<List<ReminderResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 30) =>
        Ok(await _service.GetUpcomingAsync(days));

    // GET /api/reminders/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ReminderResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReminder(Guid id)
    {
        var reminder = await _service.GetByIdAsync(id);
        return reminder is null ? NotFound() : Ok(reminder);
    }

    // POST /api/reminders
    [HttpPost]
    [ProducesResponseType<ReminderResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderRequestDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetReminder), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // DELETE /api/reminders/{id}  (ya se llamo al cliente)
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReminder(Guid id) =>
        await _service.DeleteAsync(id) ? NoContent() : NotFound();
}

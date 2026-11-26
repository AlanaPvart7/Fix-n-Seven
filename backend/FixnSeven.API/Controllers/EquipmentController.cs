using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Equipment.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly EquipmentService _service;
    private readonly IEquipmentRepository _repository;
    private readonly IValidator<CreateEquipmentRequestDto> _createValidator;
    private readonly IValidator<UpdateEquipmentRequestDto> _updateValidator;

    public EquipmentController(EquipmentService service, IEquipmentRepository repository,
        IValidator<CreateEquipmentRequestDto> createValidator, IValidator<UpdateEquipmentRequestDto> updateValidator)
    {
        _service = service;
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // GET /api/equipment?identifier=&customerId=&page=1&pageSize=20
    [Authorize(Roles = "Admin,Reception")]
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<EquipmentResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEquipment([FromQuery] string? identifier, [FromQuery] Guid? customerId,
        int page = 1, int pageSize = 20)
    {
        var (items, total) = await _repository.GetPagedAsync(identifier, customerId, page, pageSize);
        return Ok(new PagedResponseDto<EquipmentResponseDto>(items, total, page, pageSize));
    }

    // GET /api/equipment/{id}  (el tecnico tambien lo ve)
    [Authorize(Roles = "Admin,Reception,Technician")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EquipmentResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipmentById(Guid id)
    {
        var equipment = await _repository.GetByIdAsync(id);
        return equipment is null ? NotFound() : Ok(equipment);
    }

    // POST /api/equipment  (HU-06)
    [Authorize(Roles = "Admin,Reception")]
    [HttpPost]
    [ProducesResponseType<EquipmentResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEquipment([FromBody] CreateEquipmentRequestDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetEquipmentById), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // PUT /api/equipment/{id}
    [Authorize(Roles = "Admin,Reception")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateEquipment(Guid id, [FromBody] UpdateEquipmentRequestDto request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var result = await _service.UpdateAsync(id, request);
        return result.IsSuccess
            ? NoContent()
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Certificates.DTOs;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Equipment.DTOs;
using FixnSeven.Application.Features.Orders;
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
    private readonly IOrderRepository _orders;
    private readonly ICertificateRepository _certificates;

    public EquipmentController(EquipmentService service, IEquipmentRepository repository,
        IValidator<CreateEquipmentRequestDto> createValidator, IValidator<UpdateEquipmentRequestDto> updateValidator,
        IOrderRepository orders, ICertificateRepository certificates)
    {
        _orders = orders;
        _certificates = certificates;
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
    [ProducesResponseType<EquipmentDetailResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipmentById(Guid id)
    {
        var equipment = await _repository.GetByIdAsync(id);
        if (equipment is null) return NotFound();

        // HU-07: historial de ordenes del equipo
        var orders = await _orders.GetByEquipmentAsync(id);
        var certificate = await _certificates.GetActiveByEquipmentAsync(id);
        return Ok(new EquipmentDetailResponseDto(equipment, orders, certificate));
    }

    // GET /api/equipment/{id}/certificates  (historial: vigentes, vencidos y revocados)
    [Authorize(Roles = "Admin,Reception,Technician")]
    [HttpGet("{id:guid}/certificates")]
    [ProducesResponseType<List<CertificateResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipmentCertificates(Guid id)
    {
        if (await _repository.GetByIdAsync(id) is null) return NotFound();
        return Ok(await _certificates.GetByEquipmentAsync(id));
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

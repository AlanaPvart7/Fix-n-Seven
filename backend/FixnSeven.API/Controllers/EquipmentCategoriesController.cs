using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Equipment.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/equipment-categories")]
[Authorize(Roles = "Admin,Reception,Technician")]
public class EquipmentCategoriesController : ControllerBase
{
    private readonly EquipmentService _service;
    private readonly IEquipmentRepository _repository;
    private readonly IValidator<CreateEquipmentCategoryRequestDto> _validator;
    private readonly IValidator<UpdateEquipmentCategoryRequestDto> _updateValidator;

    public EquipmentCategoriesController(EquipmentService service, IEquipmentRepository repository,
        IValidator<CreateEquipmentCategoryRequestDto> validator,
        IValidator<UpdateEquipmentCategoryRequestDto> updateValidator)
    {
        _updateValidator = updateValidator;
        _service = service;
        _repository = repository;
        _validator = validator;
    }

    // GET /api/equipment-categories
    [HttpGet]
    [ProducesResponseType<List<EquipmentCategoryResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories() => Ok(await _repository.GetCategoriesAsync());

    // GET /api/equipment-categories/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EquipmentCategoryResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    // POST /api/equipment-categories  (solo Admin)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType<EquipmentCategoryResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateEquipmentCategoryRequestDto request)
    {
        await _validator.ValidateAndThrowAsync(request);
        var result = await _service.CreateCategoryAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCategory), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // PUT /api/equipment-categories/{id}  (solo Admin)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateEquipmentCategoryRequestDto request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var result = await _service.UpdateCategoryAsync(id, request);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

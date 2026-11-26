using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.Employees;
using FixnSeven.Application.Features.Employees.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _service;
    private readonly IEmployeeRepository _repository;
    private readonly IValidator<CreateEmployeeRequestDto> _validator;

    public EmployeesController(EmployeeService service, IEmployeeRepository repository,
        IValidator<CreateEmployeeRequestDto> validator)
    {
        _service = service;
        _repository = repository;
        _validator = validator;
    }

    // GET /api/employees?role=Technician&page=1&pageSize=20
    [Authorize(Roles = "Admin,Reception")]
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<EmployeeResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees([FromQuery] string? role, int page = 1, int pageSize = 20)
    {
        var (items, total) = await _repository.GetPagedAsync(role, page, pageSize);
        return Ok(new PagedResponseDto<EmployeeResponseDto>(items, total, page, pageSize));
    }

    // GET /api/employees/{id}
    [Authorize(Roles = "Admin,Reception")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        var employee = await _repository.GetByIdAsync(id);
        return employee is null ? NotFound() : Ok(employee);
    }

    // POST /api/employees  (HU-03: solo el Admin crea empleados)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType<EmployeeResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetEmployee), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

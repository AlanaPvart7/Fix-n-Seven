using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.Customers;
using FixnSeven.Application.Features.Customers.DTOs;
using FixnSeven.Domain.Entities;

namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    // TEMPORAL: en el paso 8 sale del token JWT.
    private static readonly Guid EmpresaPrueba = Guid.Parse("0192f000-0000-7000-8000-00000000a001");

    private readonly ICustomerRepository _repository;
    private readonly IValidator<CreateCustomerRequestDto> _createValidator;
    private readonly IValidator<UpdateCustomerRequestDto> _updateValidator;

    public CustomersController(
        ICustomerRepository repository,
        IValidator<CreateCustomerRequestDto> createValidator,
        IValidator<UpdateCustomerRequestDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // Mapeo manual: Entidad -> DTO (como en la clase 8)
    private static CustomerResponseDto ToDto(Customer c) => new(c.Id, c.FullName, c.Phone, c.Email);

    // GET /api/customers?search=&page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<CustomerResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search, int page = 1, int pageSize = 20)
    {
        var (items, total) = await _repository.GetPagedAsync(search, page, pageSize);
        return Ok(new PagedResponseDto<CustomerResponseDto>(items.Select(ToDto).ToList(), total, page, pageSize));
    }

    // GET /api/customers/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer is null ? NotFound() : Ok(ToDto(customer));
    }

    // POST /api/customers
    [HttpPost]
    [ProducesResponseType<CustomerResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequestDto request)
    {
        var result = await _createValidator.ValidateAsync(request);
        if (!result.IsValid)
            return BadRequest(result.Errors);   // 400 con la lista de errores

        // Mapeo manual: DTO -> Entidad (solo lo permitido)
        var customer = new Customer
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            EnterpriseId = EmpresaPrueba
        };

        var created = await _repository.AddAsync(customer);
        return CreatedAtAction(nameof(GetCustomer), new { id = created.Id }, ToDto(created));   // 201
    }

    // PUT /api/customers/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequestDto request)
    {
        var result = await _updateValidator.ValidateAsync(request);
        if (!result.IsValid)
            return BadRequest(result.Errors);

        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return NotFound();   // 404

        customer.FullName = request.FullName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        await _repository.UpdateAsync(customer);

        return NoContent();   // 204 (clase 9)
    }
}
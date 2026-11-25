using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Customers;
using FixnSeven.Domain.Entities;

namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    // TEMPORAL: la empresa de prueba. En el paso 8 sale del token JWT.
    private static readonly Guid EmpresaPrueba = Guid.Parse("0192f000-0000-7000-8000-00000000a001");

    private readonly ICustomerRepository _repository;

    // Inyección de dependencias: .NET entrega el CustomerRepository automáticamente
    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    // GET /api/customers?search=&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search, int page = 1, int pageSize = 20)
    {
        var (items, total) = await _repository.GetPagedAsync(search, page, pageSize);
        return Ok(new { items, total, page, pageSize });   // 200 OK
    }

    // POST /api/customers
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.FullName))
            return BadRequest("El nombre del cliente es obligatorio.");   // 400

        customer.EnterpriseId = EmpresaPrueba;
        var created = await _repository.AddAsync(customer);

        return CreatedAtAction(nameof(GetCustomers), new { id = created.Id }, created);   // 201
    }
}
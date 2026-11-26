using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.Orders;
using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;
    private readonly IValidator<CreateOrderRequestDto> _createValidator;
    private readonly IValidator<AssignTechnicianRequestDto> _assignValidator;
    private readonly IValidator<AddServiceItemRequestDto> _itemValidator;
    private readonly IValidator<CloseOrderRequestDto> _closeValidator;

    public OrdersController(OrderService service,
        IValidator<CreateOrderRequestDto> createValidator,
        IValidator<AssignTechnicianRequestDto> assignValidator,
        IValidator<AddServiceItemRequestDto> itemValidator,
        IValidator<CloseOrderRequestDto> closeValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _assignValidator = assignValidator;
        _itemValidator = itemValidator;
        _closeValidator = closeValidator;
    }

    // GET /api/orders?status=&technicianId=&from=&to=&page=1&pageSize=20  (HU-09: el tecnico ve solo las suyas)
    [Authorize(Roles = "Admin,Reception,Technician")]
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<OrderSummaryResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] string? status, [FromQuery] Guid? technicianId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, int page = 1, int pageSize = 20)
    {
        var (items, total) = await _service.GetPagedAsync(status, technicianId, from, to, page, pageSize);
        return Ok(new PagedResponseDto<OrderSummaryResponseDto>(items, total, page, pageSize));
    }

    // GET /api/orders/{id}
    [Authorize(Roles = "Admin,Reception,Technician")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetailResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _service.GetDetailAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    // POST /api/orders  (HU-08)
    [Authorize(Roles = "Admin,Reception")]
    [HttpPost]
    [ProducesResponseType<OrderSummaryResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var result = await _service.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // PUT /api/orders/{id}/assign  (HU-08)
    [Authorize(Roles = "Admin,Reception")]
    [HttpPut("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignTechnician(Guid id, [FromBody] AssignTechnicianRequestDto request)
    {
        await _assignValidator.ValidateAndThrowAsync(request);
        var result = await _service.AssignAsync(id, request);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // POST /api/orders/{id}/items  (HU-10)
    [Authorize(Roles = "Admin,Technician")]
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType<ServiceItemResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddServiceItemRequestDto request)
    {
        await _itemValidator.ValidateAndThrowAsync(request);
        var result = await _service.AddItemAsync(id, request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetOrder), new { id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // PUT /api/orders/{id}/close  (HU-10)
    [Authorize(Roles = "Admin,Technician")]
    [HttpPut("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CloseOrder(Guid id, [FromBody] CloseOrderRequestDto request)
    {
        await _closeValidator.ValidateAndThrowAsync(request);
        var result = await _service.CloseAsync(id, request);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

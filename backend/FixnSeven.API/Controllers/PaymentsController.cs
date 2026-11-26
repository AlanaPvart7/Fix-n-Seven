using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Orders;
using FixnSeven.Application.Features.Orders.DTOs;
namespace FixnSeven.API.Controllers;

// HU-13 (Fase 2): cobros. Solo Admin y Recepcion registran y ven pagos.
[ApiController]
[Authorize(Roles = "Admin,Reception")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _service;
    private readonly IValidator<CreatePaymentRequestDto> _validator;

    public PaymentsController(PaymentService service, IValidator<CreatePaymentRequestDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    // POST /api/orders/{id}/payments
    [HttpPost("api/orders/{id:guid}/payments")]
    [ProducesResponseType<PaymentResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePayment(Guid id, [FromBody] CreatePaymentRequestDto request)
    {
        await _validator.ValidateAndThrowAsync(request);
        var result = await _service.CreateAsync(id, request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPayment), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // GET /api/orders/{id}/payments
    [HttpGet("api/orders/{id:guid}/payments")]
    [ProducesResponseType<List<PaymentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderPayments(Guid id)
    {
        var payments = await _service.GetByOrderAsync(id);
        return payments is null ? NotFound() : Ok(payments);
    }

    // GET /api/payments/{id}
    [HttpGet("api/payments/{id:guid}")]
    [ProducesResponseType<PaymentResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _service.GetByIdAsync(id);
        return payment is null ? NotFound() : Ok(payment);
    }
}

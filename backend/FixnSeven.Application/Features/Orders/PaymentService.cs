using FixnSeven.Application.Common;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Orders;

// HU-13 (Fase 2): cobros de la orden. Varios pagos por orden (anticipo + resto)
public class PaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IOrderRepository _orders;
    private readonly ICurrentTenant _tenant;

    public PaymentService(IPaymentRepository repository, IOrderRepository orders, ICurrentTenant tenant)
    {
        _repository = repository;
        _orders = orders;
        _tenant = tenant;
    }

    public async Task<ServiceResult<PaymentResponseDto>> CreateAsync(Guid orderId, CreatePaymentRequestDto request)
    {
        var employeeId = _tenant.EmployeeId ?? throw new InvalidOperationException("No hay EmployeeId en el token.");

        if (await _orders.GetEntityAsync(orderId) is null)
            return ServiceResult<PaymentResponseDto>.Fail(404, "La orden no existe.");

        var balance = await _repository.GetOrderTotalAsync(orderId) - await _repository.GetOrderPaidAsync(orderId);
        if (balance <= 0)
            return ServiceResult<PaymentResponseDto>.Fail(409, "La orden no tiene saldo pendiente.");
        if (request.Amount > balance)
            return ServiceResult<PaymentResponseDto>.Fail(400,
                $"El pago ({request.Amount:N2}) es mayor que el saldo pendiente ({balance:N2}).");

        var payment = new Payment
        {
            ServiceOrderId = orderId,
            ReceivedByEmployeeId = employeeId,           // sale del JWT
            Amount = request.Amount,
            Method = request.Method,
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim()
        };
        await _repository.AddAsync(payment);

        var dto = await _repository.GetByIdAsync(payment.Id);
        return ServiceResult<PaymentResponseDto>.Ok(dto!, 201);
    }

    public async Task<List<PaymentResponseDto>?> GetByOrderAsync(Guid orderId) =>
        await _orders.GetEntityAsync(orderId) is null ? null : await _repository.GetByOrderAsync(orderId);

    public Task<PaymentResponseDto?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
}

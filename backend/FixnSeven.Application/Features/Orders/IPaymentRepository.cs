using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Orders;

public interface IPaymentRepository
{
    Task<decimal> GetOrderTotalAsync(Guid orderId);
    Task<decimal> GetOrderPaidAsync(Guid orderId);
    Task AddAsync(Payment payment);
    Task<List<PaymentResponseDto>> GetByOrderAsync(Guid orderId);
    Task<PaymentResponseDto?> GetByIdAsync(Guid id);
}

using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Orders;

public interface IOrderRepository
{
    Task<(bool Exists, Guid? CustomerId)> GetEquipmentOwnerAsync(Guid equipmentId);
    Task<bool> TechnicianExistsAsync(Guid employeeId);        // empleado con rol Technician de mi empresa
    Task<int> CountOrdersSinceAsync(DateTime sinceUtc);
    Task<int?> GetPlanMaxMonthlyOrdersAsync(Guid enterpriseId);

    Task AddAsync(ServiceOrder order);
    Task<ServiceOrder?> GetEntityAsync(Guid id);
    Task UpdateAsync(ServiceOrder order);
    Task AddItemAsync(ServiceItem item);

    Task<(List<OrderSummaryResponseDto> Items, int Total)> GetPagedAsync(
        string? status, Guid? technicianId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize);
    Task<OrderSummaryResponseDto?> GetSummaryAsync(Guid id);
    Task<OrderDetailResponseDto?> GetDetailAsync(Guid id);
    Task<List<OrderSummaryResponseDto>> GetByEquipmentAsync(Guid equipmentId);
}

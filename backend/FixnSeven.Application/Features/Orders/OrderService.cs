using FixnSeven.Application.Common;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Application.Features.Certificates;
using FixnSeven.Application.Features.Orders.DTOs;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Orders;

// Reglas de negocio de HU-08, HU-09 y HU-10
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICurrentTenant _tenant;
    private readonly ICertificateRepository _certificates;

    public OrderService(IOrderRepository repository, ICurrentTenant tenant, ICertificateRepository certificates)
    {
        _repository = repository;
        _tenant = tenant;
        _certificates = certificates;
    }

    private bool IsTechnician => _tenant.Role == "Technician";

    // Un tecnico solo puede tocar las ordenes que tiene asignadas
    private bool IsMine(ServiceOrder order) => !IsTechnician || order.TechnicianId == _tenant.EmployeeId;

    // ---------- HU-08: abrir orden ----------
    public async Task<ServiceResult<OrderSummaryResponseDto>> CreateAsync(CreateOrderRequestDto request)
    {
        var enterpriseId = _tenant.EnterpriseId ?? throw new InvalidOperationException("No hay EnterpriseId en el token.");

        var (exists, customerId) = await _repository.GetEquipmentOwnerAsync(request.EquipmentId);
        if (!exists)
            return ServiceResult<OrderSummaryResponseDto>.Fail(400, "El equipo no existe.");

        if (request.TechnicianId is not null && !await _repository.TechnicianExistsAsync(request.TechnicianId.Value))
            return ServiceResult<OrderSummaryResponseDto>.Fail(404, "El técnico no existe en su empresa.");

        // Limite de ordenes por mes del plan
        var maxOrders = await _repository.GetPlanMaxMonthlyOrdersAsync(enterpriseId);
        if (maxOrders is not null)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            if (await _repository.CountOrdersSinceAsync(monthStart) >= maxOrders)
                return ServiceResult<OrderSummaryResponseDto>.Fail(409,
                    $"Su plan permite {maxOrders} órdenes al mes. Cambie de plan para abrir más.");
        }

        var order = new ServiceOrder
        {
            EquipmentId = request.EquipmentId,
            CustomerId = customerId,                       // foto del dueno en ese momento
            TechnicianId = request.TechnicianId,
            Status = request.TechnicianId is null ? OrderStatus.Received : OrderStatus.InProgress,
            ReportedIssue = request.ReportedIssue.Trim(),
            ReceivedCondition = request.ReceivedCondition?.Trim()
        };
        await _repository.AddAsync(order);

        var summary = await _repository.GetSummaryAsync(order.Id);
        return ServiceResult<OrderSummaryResponseDto>.Ok(summary!, 201);
    }

    // ---------- HU-08: asignar tecnico ----------
    public async Task<ServiceResult<bool>> AssignAsync(Guid orderId, AssignTechnicianRequestDto request)
    {
        var order = await _repository.GetEntityAsync(orderId);
        if (order is null) return ServiceResult<bool>.Fail(404, "La orden no existe.");
        if (order.Status == OrderStatus.Closed) return ServiceResult<bool>.Fail(409, "La orden ya está cerrada.");
        if (!await _repository.TechnicianExistsAsync(request.TechnicianId))
            return ServiceResult<bool>.Fail(404, "El técnico no existe en su empresa.");

        order.TechnicianId = request.TechnicianId;
        order.Status = OrderStatus.InProgress;
        await _repository.UpdateAsync(order);
        return ServiceResult<bool>.Ok(true, 204);
    }

    // ---------- HU-10: agregar trabajo o repuesto ----------
    public async Task<ServiceResult<ServiceItemResponseDto>> AddItemAsync(Guid orderId, AddServiceItemRequestDto request)
    {
        var order = await _repository.GetEntityAsync(orderId);
        if (order is null) return ServiceResult<ServiceItemResponseDto>.Fail(404, "La orden no existe.");
        if (!IsMine(order)) return ServiceResult<ServiceItemResponseDto>.Fail(403, "Esta orden no está asignada a usted.");
        if (order.Status == OrderStatus.Closed)
            return ServiceResult<ServiceItemResponseDto>.Fail(409, "La orden ya está cerrada; no se pueden agregar trabajos.");

        var item = new ServiceItem
        {
            ServiceOrderId = order.Id,
            Kind = request.Kind,
            Description = request.Description.Trim(),
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };
        await _repository.AddItemAsync(item);

        return ServiceResult<ServiceItemResponseDto>.Ok(new ServiceItemResponseDto(
            item.Id, item.Kind, item.Description, item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice), 201);
    }

    // ---------- HU-10: cerrar orden ----------
    public async Task<ServiceResult<bool>> CloseAsync(Guid orderId, CloseOrderRequestDto request)
    {
        var order = await _repository.GetEntityAsync(orderId);
        if (order is null) return ServiceResult<bool>.Fail(404, "La orden no existe.");
        if (!IsMine(order)) return ServiceResult<bool>.Fail(403, "Esta orden no está asignada a usted.");
        if (order.Status == OrderStatus.Closed) return ServiceResult<bool>.Fail(409, "La orden ya está cerrada.");
        if (order.TechnicianId is null) return ServiceResult<bool>.Fail(409, "Asigne un técnico antes de cerrar la orden.");

        order.Status = OrderStatus.Closed;
        order.WorkDone = request.WorkDone.Trim();
        order.Notes = request.Notes?.Trim();
        order.ClosedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(order);
        return ServiceResult<bool>.Ok(true, 204);
    }

    // ---------- HU-09: consultar ----------
    public Task<(List<OrderSummaryResponseDto> Items, int Total)> GetPagedAsync(
        string? status, Guid? technicianId, DateTime? from, DateTime? to, int page, int pageSize)
    {
        // El tecnico SOLO ve las suyas, aunque mande otro technicianId
        if (IsTechnician) technicianId = _tenant.EmployeeId;

        DateTime? fromUtc = from is null ? null : DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
        DateTime? toUtc = to is null ? null : DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
        return _repository.GetPagedAsync(status, technicianId, fromUtc, toUtc, page, pageSize);
    }

    public async Task<OrderDetailResponseDto?> GetDetailAsync(Guid id)
    {
        var detail = await _repository.GetDetailAsync(id);
        if (detail is null) return null;
        if (IsTechnician && detail.TechnicianId != _tenant.EmployeeId) return null;   // para el, "no existe"
        return detail with { Certificate = await _certificates.GetLatestByOrderAsync(id) };
    }
}

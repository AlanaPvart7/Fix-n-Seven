using FixnSeven.Application.Features.Equipment.DTOs;
using EquipmentEntity = FixnSeven.Domain.Entities.Equipment;
using FixnSeven.Domain.Entities;
namespace FixnSeven.Application.Features.Equipment;

public interface IEquipmentRepository
{
    // Categorias
    Task<List<EquipmentCategoryResponseDto>> GetCategoriesAsync();
    Task<EquipmentCategoryResponseDto?> GetCategoryByIdAsync(Guid id);
    Task<bool> CategoryNameExistsAsync(string name);
    Task<bool> CategoryExistsAsync(Guid id);
    Task AddCategoryAsync(EquipmentCategory category);
    Task<EquipmentCategory?> GetCategoryEntityAsync(Guid id);
    Task<bool> CategoryNameExistsAsync(string name, Guid excludeId);
    Task UpdateCategoryAsync(EquipmentCategory category);

    // Equipos
    Task<bool> CustomerExistsAsync(Guid customerId);
    Task<bool> IdentifierExistsAsync(string identifier, Guid? exceptEquipmentId = null);
    Task AddAsync(EquipmentEntity equipment);
    Task<EquipmentEntity?> GetEntityAsync(Guid id);
    Task UpdateAsync(EquipmentEntity equipment);
    Task<(List<EquipmentResponseDto> Items, int Total)> GetPagedAsync(string? identifier, Guid? customerId, int page, int pageSize);
    Task<EquipmentResponseDto?> GetByIdAsync(Guid id);
}

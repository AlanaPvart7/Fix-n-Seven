using FixnSeven.Application.Common;
using FixnSeven.Application.Features.Equipment.DTOs;
using FixnSeven.Domain.Entities;
using EquipmentEntity = FixnSeven.Domain.Entities.Equipment;
namespace FixnSeven.Application.Features.Equipment;

// Reglas de negocio de HU-06: categoria y cliente de MI empresa, identificador unico por empresa
public class EquipmentService
{
    private readonly IEquipmentRepository _repository;
    public EquipmentService(IEquipmentRepository repository) => _repository = repository;

    public async Task<ServiceResult<EquipmentCategoryResponseDto>> CreateCategoryAsync(CreateEquipmentCategoryRequestDto request)
    {
        var name = request.Name.Trim();
        if (await _repository.CategoryNameExistsAsync(name))
            return ServiceResult<EquipmentCategoryResponseDto>.Fail(409, "Ya existe una categoría con ese nombre.");

        var category = new EquipmentCategory
        {
            Name = name,
            IdentifierLabel = request.IdentifierLabel.Trim(),
            UsageUnit = request.UsageUnit
        };
        await _repository.AddCategoryAsync(category);

        return ServiceResult<EquipmentCategoryResponseDto>.Ok(
            new EquipmentCategoryResponseDto(category.Id, category.Name, category.IdentifierLabel, category.UsageUnit), 201);
    }

    public async Task<ServiceResult<EquipmentResponseDto>> CreateAsync(CreateEquipmentRequestDto request)
    {
        var error = await CheckReferencesAsync(request.CategoryId, request.CustomerId, request.Identifier.Trim(), null);
        if (error is not null) return ServiceResult<EquipmentResponseDto>.Fail(error.Value.Code, error.Value.Message);

        var equipment = new EquipmentEntity
        {
            CustomerId = request.CustomerId,
            CategoryId = request.CategoryId,
            Identifier = request.Identifier.Trim(),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            CurrentUsage = request.CurrentUsage
        };
        await _repository.AddAsync(equipment);

        var dto = await _repository.GetByIdAsync(equipment.Id);
        return ServiceResult<EquipmentResponseDto>.Ok(dto!, 201);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(Guid id, UpdateEquipmentRequestDto request)
    {
        var equipment = await _repository.GetEntityAsync(id);
        if (equipment is null) return ServiceResult<bool>.Fail(404, "El equipo no existe.");

        var error = await CheckReferencesAsync(request.CategoryId, request.CustomerId, request.Identifier.Trim(), id);
        if (error is not null) return ServiceResult<bool>.Fail(error.Value.Code, error.Value.Message);

        equipment.CustomerId = request.CustomerId;
        equipment.CategoryId = request.CategoryId;
        equipment.Identifier = request.Identifier.Trim();
        equipment.Brand = request.Brand.Trim();
        equipment.Model = request.Model.Trim();
        equipment.CurrentUsage = request.CurrentUsage;
        await _repository.UpdateAsync(equipment);

        return ServiceResult<bool>.Ok(true, 204);
    }

    // Las consultas ya vienen filtradas por empresa: si es de otra empresa, "no existe"
    private async Task<(int Code, string Message)?> CheckReferencesAsync(Guid categoryId, Guid? customerId, string identifier, Guid? exceptId)
    {
        if (!await _repository.CategoryExistsAsync(categoryId))
            return (400, "La categoría no existe.");
        if (customerId is not null && !await _repository.CustomerExistsAsync(customerId.Value))
            return (400, "El cliente no existe.");
        if (await _repository.IdentifierExistsAsync(identifier, exceptId))
            return (409, "Ya hay un equipo con ese identificador en su empresa.");
        return null;
    }
}

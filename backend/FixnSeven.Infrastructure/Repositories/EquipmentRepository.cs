using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Equipment;
using FixnSeven.Application.Features.Equipment.DTOs;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;
using EquipmentEntity = FixnSeven.Domain.Entities.Equipment;
namespace FixnSeven.Infrastructure.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly FixnSevenDbContext _context;
    public EquipmentRepository(FixnSevenDbContext context) => _context = context;

    // ---------- Categorias ----------
    public Task<List<EquipmentCategoryResponseDto>> GetCategoriesAsync() =>
        _context.EquipmentCategories
            .OrderBy(c => c.Name)
            .Select(c => new EquipmentCategoryResponseDto(c.Id, c.Name, c.IdentifierLabel, c.UsageUnit))
            .ToListAsync();

    public Task<EquipmentCategoryResponseDto?> GetCategoryByIdAsync(Guid id) =>
        _context.EquipmentCategories
            .Where(c => c.Id == id)
            .Select(c => new EquipmentCategoryResponseDto(c.Id, c.Name, c.IdentifierLabel, c.UsageUnit))
            .FirstOrDefaultAsync();

    public Task<bool> CategoryNameExistsAsync(string name) =>
        _context.EquipmentCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower());

    public Task<EquipmentCategory?> GetCategoryEntityAsync(Guid id) =>
        _context.EquipmentCategories.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> CategoryNameExistsAsync(string name, Guid excludeId) =>
        _context.EquipmentCategories.AnyAsync(c => c.Id != excludeId && c.Name.ToLower() == name.ToLower());

    public async Task UpdateCategoryAsync(EquipmentCategory category)
    {
        _context.EquipmentCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public Task<bool> CategoryExistsAsync(Guid id) =>
        _context.EquipmentCategories.AnyAsync(c => c.Id == id);

    public async Task AddCategoryAsync(EquipmentCategory category)
    {
        await _context.EquipmentCategories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    // ---------- Equipos ----------
    public Task<bool> CustomerExistsAsync(Guid customerId) =>
        _context.Customers.AnyAsync(c => c.Id == customerId);

    public Task<bool> IdentifierExistsAsync(string identifier, Guid? exceptEquipmentId = null) =>
        _context.Equipment.AnyAsync(e => e.Identifier == identifier && e.Id != exceptEquipmentId);

    public async Task AddAsync(EquipmentEntity equipment)
    {
        await _context.Equipment.AddAsync(equipment);
        await _context.SaveChangesAsync();
    }

    public Task<EquipmentEntity?> GetEntityAsync(Guid id) =>
        _context.Equipment.FirstOrDefaultAsync(e => e.Id == id);

    public async Task UpdateAsync(EquipmentEntity equipment)
    {
        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync();
    }

    // Consulta base: equipo + categoria + cliente (LEFT JOIN, porque el cliente es opcional)
    private IQueryable<EquipmentResponseDto> Query(IQueryable<EquipmentEntity> source) =>
        from e in source
        join c in _context.EquipmentCategories on e.CategoryId equals c.Id
        join cu in _context.Customers on e.CustomerId equals (Guid?)cu.Id into customers
        from cu in customers.DefaultIfEmpty()
        select new EquipmentResponseDto(
            e.Id, e.CustomerId, cu != null ? cu.FullName : null,
            e.CategoryId, c.Name, c.IdentifierLabel,
            e.Identifier, e.Brand, e.Model, e.CurrentUsage);

    public async Task<(List<EquipmentResponseDto> Items, int Total)> GetPagedAsync(
        string? identifier, Guid? customerId, int page, int pageSize)
    {
        var source = _context.Equipment.AsQueryable();
        if (!string.IsNullOrWhiteSpace(identifier))
            source = source.Where(e => e.Identifier.Contains(identifier));
        if (customerId is not null)
            source = source.Where(e => e.CustomerId == customerId);

        // Contar, ordenar y paginar sobre la ENTIDAD (EF lo traduce a SQL);
        // despues se arma el DTO con los JOIN solo de esa pagina
        var total = await source.CountAsync();
        var pageSource = source
            .OrderBy(e => e.Brand).ThenBy(e => e.Model)
            .Skip((page - 1) * pageSize).Take(pageSize);

        var items = (await Query(pageSource).ToListAsync())
            .OrderBy(x => x.Brand).ThenBy(x => x.Model)   // el JOIN no garantiza el orden
            .ToList();
        return (items, total);
    }

    public Task<EquipmentResponseDto?> GetByIdAsync(Guid id) =>
        Query(_context.Equipment.Where(e => e.Id == id)).FirstOrDefaultAsync();
}

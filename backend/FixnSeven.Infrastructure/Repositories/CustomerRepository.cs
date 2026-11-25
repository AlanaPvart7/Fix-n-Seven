using Microsoft.EntityFrameworkCore;
using FixnSeven.Application.Features.Customers;
using FixnSeven.Domain.Entities;
using FixnSeven.Infrastructure.Data;

namespace FixnSeven.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly FixnSevenDbContext _context;

    public CustomerRepository(FixnSevenDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Customer> Items, int Total)> GetPagedAsync(string? search, int page, int pageSize)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.FullName.Contains(search) || c.Phone.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<Customer?> GetByIdAsync(Guid id) =>
        _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Customer> AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();   // transacción SQL (COMMIT)
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }
}
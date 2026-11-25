using FixnSeven.Domain.Entities;

namespace FixnSeven.Application.Features.Customers;

public interface ICustomerRepository
{
    Task<(List<Customer> Items, int Total)> GetPagedAsync(string? search, int page, int pageSize);
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
}
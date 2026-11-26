using FixnSeven.Application.Features.Enterprises.DTOs;
namespace FixnSeven.Application.Features.Enterprises;

public interface ICatalogRepository
{
    Task<List<EnterpriseTypeResponseDto>> GetEnterpriseTypesAsync();
    Task<List<PlanResponseDto>> GetPlansAsync();
    Task<EnterpriseResponseDto?> GetEnterpriseAsync(Guid enterpriseId);
}

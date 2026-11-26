using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.Enterprises;
using FixnSeven.Application.Features.Enterprises.DTOs;
namespace FixnSeven.API.Controllers;

// Catalogos globales y publicos: los usa el formulario de registro (sin token)
[ApiController]
[AllowAnonymous]
public class CatalogsController : ControllerBase
{
    private readonly ICatalogRepository _repository;
    public CatalogsController(ICatalogRepository repository) => _repository = repository;

    // GET /api/enterprise-types
    [HttpGet("api/enterprise-types")]
    [ProducesResponseType<List<EnterpriseTypeResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnterpriseTypes() => Ok(await _repository.GetEnterpriseTypesAsync());

    // GET /api/plans
    [HttpGet("api/plans")]
    [ProducesResponseType<List<PlanResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans() => Ok(await _repository.GetPlansAsync());
}

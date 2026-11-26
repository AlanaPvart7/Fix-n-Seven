using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Features.CustomerDevices;
namespace FixnSeven.API.Controllers;

// Fotos que el dueno decidio mostrar en la consulta publica del QR (los PDF nunca son publicos)
[ApiController]
[AllowAnonymous]
public class PublicDeviceFilesController : ControllerBase
{
    private readonly CustomerAccountService _service;
    public PublicDeviceFilesController(CustomerAccountService service) => _service = service;

    // GET /api/public/device-files/{id}
    [HttpGet("api/public/device-files/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicPhoto(Guid id)
    {
        var photo = await _service.GetPublicPhotoAsync(id);
        return photo is null ? NotFound() : File(photo.Value.Content, photo.Value.ContentType);
    }
}

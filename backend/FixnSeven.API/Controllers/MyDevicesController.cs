using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixnSeven.Application.Common.DTOs;
using FixnSeven.Application.Features.CustomerDevices;
using FixnSeven.Application.Features.CustomerDevices.DTOs;
namespace FixnSeven.API.Controllers;

// Fase 2: los equipos del comprador. Cada cuenta solo ve y edita LOS SUYOS (regla 14).
[ApiController]
[Route("api/me/devices")]
[Authorize(Roles = "Customer")]
public class MyDevicesController : ControllerBase
{
    private readonly CustomerAccountService _service;
    private readonly IValidator<LinkDeviceRequestDto> _linkValidator;
    private readonly IValidator<UpdateCustomerDeviceRequestDto> _updateValidator;

    public MyDevicesController(CustomerAccountService service,
        IValidator<LinkDeviceRequestDto> linkValidator, IValidator<UpdateCustomerDeviceRequestDto> updateValidator)
    {
        _service = service;
        _linkValidator = linkValidator;
        _updateValidator = updateValidator;
    }

    // POST /api/me/devices  (vincular con el codigo del QR + codigo privado o de transferencia)
    [HttpPost]
    [ProducesResponseType<CustomerDeviceResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LinkDevice([FromBody] LinkDeviceRequestDto request)
    {
        await _linkValidator.ValidateAndThrowAsync(request);
        var result = await _service.LinkAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetDevice), new { id = result.Data!.Id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // GET /api/me/devices?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<CustomerDeviceResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevices(int page = 1, int pageSize = 20)
    {
        var (items, total) = await _service.GetMyDevicesAsync(page, pageSize);
        return Ok(new PagedResponseDto<CustomerDeviceResponseDto>(items, total, page, pageSize));
    }

    // GET /api/me/devices/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerDeviceResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDevice(Guid id)
    {
        var device = await _service.GetMyDeviceAsync(id);
        return device is null ? NotFound() : Ok(device);
    }

    // PUT /api/me/devices/{id}  (excepcion del contrato: 200 con warnings)
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CustomerDeviceResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] UpdateCustomerDeviceRequestDto request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var result = await _service.UpdateAsync(id, request);
        return result.IsSuccess ? Ok(result.Data) : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // POST /api/me/devices/{id}/files  (multipart: "file" y "isPublic")
    [HttpPost("{id:guid}/files")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<CustomerDeviceFileResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddFile(Guid id, IFormFile file, [FromForm] bool isPublic = false)
    {
        await using var stream = file.OpenReadStream();
        var result = await _service.AddFileAsync(id, file.FileName, stream, isPublic);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetDevice), new { id }, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    // DELETE /api/me/devices/{id}/files/{fileId}
    [HttpDelete("{id:guid}/files/{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(Guid id, Guid fileId) =>
        await _service.DeleteFileAsync(id, fileId) ? NoContent() : NotFound();

    // POST /api/me/devices/{id}/transfer  (codigo de un solo uso para el nuevo dueno)
    [HttpPost("{id:guid}/transfer")]
    [ProducesResponseType<TransferCodeResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTransferCode(Guid id)
    {
        var result = await _service.CreateTransferCodeAsync(id);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Data)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}

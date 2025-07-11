using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Vendors;
using Tender.Application.Queries.Vendors;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/vendors")]
public sealed class VendorsController : ControllerBase
{
    private readonly IMediator _med;
    public VendorsController(IMediator med) => _med = med;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorCommand body,
                                            CancellationToken ct)
    {
        var id = await _med.Send(body, ct);
        return Created($"/api/vendors/{id}", new { id });  
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] bool includeBidSummary = false,
                                          CancellationToken ct = default)
    {
        var list = await _med.Send(new GetVendorsQuery(includeBidSummary), ct);
        return Ok(list);
    }

    [HttpGet("{id}", Name = "GetVendorById")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _med.Send(new GetVendorDetailsQuery(id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }


}

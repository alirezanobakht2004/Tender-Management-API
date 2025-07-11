using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Bids;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/bids")]
public sealed class BidsController : ControllerBase
{
    private readonly IMediator _med;
    public BidsController(IMediator med) => _med = med;

    [HttpPost]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBidCommand body,
                                            CancellationToken ct)
    {
        var id = await _med.Send(body, ct);
        return Created($"/api/bids/{id}", new { id });
    }
}

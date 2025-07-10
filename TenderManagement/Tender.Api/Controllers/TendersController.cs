// Tender.Api/Controllers/TendersController.cs
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Tenders;
using Tender.Api.Extensions;          

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TendersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TendersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTenderCommand command,
                                            CancellationToken cancellationToken)
    {
        // Enrich command with user id from JWT
        var enriched = command with { CreatedByUserId = User.GetUserId() };
        var id = await _mediator.Send(enriched, cancellationToken);
        return CreatedAtRoute("GetTenderById", new { id }, null);
    }
}


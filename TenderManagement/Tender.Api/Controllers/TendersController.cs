// Tender.Api/Controllers/TendersController.cs
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Tenders;
using Tender.Api.Extensions;
using Tender.Application.Queries.Tenders;

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
        return CreatedAtRoute("GetTenderById", new { id }, new { id });
    }


    [HttpGet("{id}", Name = "GetTenderById")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(new GetTenderDetailsQuery(id), cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _mediator.Send(new GetTenderListQuery(), ct);
        return Ok(list);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id,
                                        [FromBody] UpdateTenderCommand body,
                                        CancellationToken ct)
    {
        if (id != body.Id) return BadRequest("Id mismatch");
        await _mediator.Send(body, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTenderCommand(id), ct);
        return NoContent();
    }

}




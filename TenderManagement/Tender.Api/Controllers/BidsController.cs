// Tender.Api/Controllers/BidsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Bids;
using Tender.Application.Dtos;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/bids")]
public sealed class BidsController : ControllerBase
{
    private readonly IMediator _med;
    public BidsController(IMediator med) => _med = med;

    // ──────────────────────────────────────────────────────────────────────────────
    // POST /api/bids
    // ──────────────────────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBidCommand body,
                                        CancellationToken ct)
    {
        try
        {
            var id = await _med.Send(body, ct);
            return Created($"/api/bids/{id}", new { id });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                           .GroupBy(e => e.PropertyName)
                           .ToDictionary(g => g.Key,
                                         g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new ValidationProblemDetails(errors));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Resource not found",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = ex.Message
            });
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // PUT /api/bids/{id}/status
    // ──────────────────────────────────────────────────────────────────────────────
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id,
                                                  [FromBody] UpdateBidStatusDto body,
                                                  CancellationToken ct)
    {
        try
        {
            await _med.Send(new UpdateBidStatusCommand(id, body.StatusId), ct);
            return NoContent();
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                           .GroupBy(e => e.PropertyName)
                           .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Validation error",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Resource Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            });
        }
    }
}

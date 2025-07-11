using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Tenders;
using Tender.Api.Extensions;
using Tender.Application.Queries.Tenders;
using FluentValidation;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TendersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TendersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTenderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var enriched = command with { CreatedByUserId = User.GetUserId() };
            var id = await _mediator.Send(enriched, cancellationToken);
            return CreatedAtRoute("GetTenderById", new { id }, new { id });
        }
        catch (ValidationException ex)
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
        if (id != body.Id)
            return NotFound(new ProblemDetails
            {
                Title = "Tender not found",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            });

        try
        {
            await _mediator.Send(body, ct);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                           .GroupBy(e => e.PropertyName)
                           .ToDictionary(g => g.Key,
                                         g => g.Select(e => e.ErrorMessage).ToArray());

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
                Title = "Tender not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                });
        }
    }



    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTenderCommand(id), ct);
        return NoContent();
    }
}

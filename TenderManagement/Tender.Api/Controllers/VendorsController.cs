using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Commands.Vendors;
using Tender.Application.Queries.Vendors;
using FluentValidation;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/vendors")]
public sealed class VendorsController : ControllerBase
{
    private readonly IMediator _med;
    public VendorsController(IMediator med) => _med = med;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorCommand body, CancellationToken ct)
    {
        try
        {
            var id = await _med.Send(body, ct);
            return Created($"/api/vendors/{id}", new { id });
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

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] bool includeBidSummary = false, CancellationToken ct = default)
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

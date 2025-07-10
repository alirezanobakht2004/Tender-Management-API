// Tender.Api/Controllers/LookupsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Queries.Lookups;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class LookupsController : ControllerBase
{
    private readonly IMediator _med;
    public LookupsController(IMediator med) => _med = med;

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct) =>
        Ok(await _med.Send(new GetCategoriesQuery(), ct));

    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses(CancellationToken ct) =>
        Ok(await _med.Send(new GetStatusesQuery(), ct));
}

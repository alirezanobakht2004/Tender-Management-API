using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Auth.Dto;
using Tender.Application.Auth;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _med;
    public AuthController(IMediator med) => _med = med;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand cmd,
                                              CancellationToken ct)
    {
        var id = await _med.Send(cmd, ct);
        return Created(string.Empty, new { id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginQuery qry,
                                           CancellationToken ct)
    {
        var token = await _med.Send(qry, ct);
        return Ok(new { token });
    }
}

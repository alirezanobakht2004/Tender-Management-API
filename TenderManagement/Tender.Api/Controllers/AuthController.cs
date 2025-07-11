using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tender.Application.Auth;
using Tender.Application.Auth.Dto;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _med;
    public AuthController(IMediator med) => _med = med;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand cmd,
        CancellationToken ct)
    {
        try
        {
            var id = await _med.Send(cmd, ct);
            return Created(string.Empty, new { id });
        }
        catch (DuplicateEmailException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Duplicate email",
                Status = StatusCodes.Status409Conflict,
                Detail = "A user with this email already exists.",
                Type = "/problems/duplicate-email"
            });
        }
        catch (FluentValidation.ValidationException ex)
        {
            // Group validation errors by property
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Type = "/problems/validation-error"
            });
        }
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginQuery qry,
        CancellationToken ct)
    {
        var token = await _med.Send(qry, ct);

        if (token is null)
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",   // ← Title (capital T)
                Status = StatusCodes.Status401Unauthorized,
                Type = "/problems/unauthorized"
            });

        return Ok(new { token });
    }
}

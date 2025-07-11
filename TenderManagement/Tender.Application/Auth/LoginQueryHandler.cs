using MediatR;
using Microsoft.AspNetCore.Identity;
using Tender.Domain.Contracts.Repositories;
using Tender.Application.Auth.Dto;
using Tender.Domain.Entities;

namespace Tender.Application.Auth;

public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, string?>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IJwtTokenService _tokens;

    public LoginQueryHandler(IUserRepository users,
                             IPasswordHasher<User> hasher,
                             IJwtTokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<string?> Handle(LoginQuery req, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(req.Email, ct);
        if (user is null)
            return null;

        var ok = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        return ok == PasswordVerificationResult.Success
               ? _tokens.Generate(user)
               : null;
    }
}

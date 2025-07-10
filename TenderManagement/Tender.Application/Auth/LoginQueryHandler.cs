using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Tender.Application.Auth.Dto;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Application.Auth;

public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, string>
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

    public async Task<string> Handle(LoginQuery req, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(req.Email, ct)
                   ?? throw new UnauthorizedAccessException("Invalid credentials");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        return _tokens.Generate(user);
    }
}

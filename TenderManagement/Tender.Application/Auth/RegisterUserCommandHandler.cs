using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Tender.Application.Auth.Dto;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Application.Auth;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<User> _hasher;

    public RegisterUserCommandHandler(IUserRepository users,
                                      IUnitOfWork uow,
                                      IPasswordHasher<User> hasher)
    {
        _users = users;
        _uow = uow;
        _hasher = hasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand req, CancellationToken ct)
    {
        var user = new User(req.Email, string.Empty, req.Role);
        user.SetPasswordHash(_hasher.HashPassword(user, req.Password));

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return user.Id;
    }
}

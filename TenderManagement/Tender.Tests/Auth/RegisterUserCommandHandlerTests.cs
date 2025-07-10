using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Tender.Application.Auth;
using Tender.Application.Auth.Dto;
using Tender.Domain.Contracts;
using Tender.Infrastructure.Persistence;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;

namespace Tender.Tests.Auth;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Persists_user_and_returns_id()
    {
        using var db = InMemoryDbFactory.Create();
        var repo = new UserRepository(db);
        var uow = new UnitOfWork(db);
        var hasher = new PasswordHasher<Tender.Domain.Entities.User>();
        var handler = new RegisterUserCommandHandler(repo, uow, hasher);

        var cmd = new RegisterUserCommand("test@demo.io", "Secret1!", "Admin");
        var id = await handler.Handle(cmd, default);

        id.Should().NotBe(Guid.Empty);
        (await db.Users.FindAsync(id)).Should().NotBeNull();
    }
}

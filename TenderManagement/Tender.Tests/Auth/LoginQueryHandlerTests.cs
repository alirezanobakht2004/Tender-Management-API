// Tests/Auth/LoginQueryHandlerTests.cs  ← pure unit
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Tender.Application.Auth;
using Tender.Application.Auth.Dto;
using Tender.Domain.Entities;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;
using FluentAssertions;

namespace Tender.Tests.Auth;

public sealed class LoginQueryHandlerTests
{
    [Fact]
    public async Task Returns_token_for_valid_credentials()
    {
        await using var db = InMemoryDbFactory.Create();
        var hasher = new PasswordHasher<User>();
        var user = new User("valid@demo.io", string.Empty, "Admin");
        user.SetPasswordHash(hasher.HashPassword(user, "Secret1!"));
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repo = new UserRepository(db);
        var tokenSvc = new StubJwtTokenService();
        var handler = new LoginQueryHandler(repo, hasher, tokenSvc);

        var jwt = await handler.Handle(new LoginQuery("valid@demo.io", "Secret1!"), default);

        jwt.Should().Be("stub-jwt-token");
        tokenSvc.LastUserEmail.Should().Be("valid@demo.io");
    }
}

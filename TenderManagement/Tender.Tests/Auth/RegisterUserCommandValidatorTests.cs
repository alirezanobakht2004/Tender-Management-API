using System;
using Tender.Application.Auth;
using Tender.Application.Auth.Dto;
using Xunit;
using FluentAssertions;

namespace Tender.Tests.Auth;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _v = new();

    [Fact]
    public void Rejects_invalid_email()
    {
        var cmd = new RegisterUserCommand("badmail", "Secret1!", "Admin");
        _v.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accepts_valid_input()
    {
        var cmd = new RegisterUserCommand("good@demo.io", "Secret1!", "Vendor");
        _v.Validate(cmd).IsValid.Should().BeTrue();
    }
}

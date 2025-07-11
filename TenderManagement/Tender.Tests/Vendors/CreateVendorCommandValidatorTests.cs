using FluentAssertions;
using Tender.Application.Commands.Vendors;
using Xunit;

public sealed class CreateVendorCommandValidatorTests
{
    private readonly CreateVendorCommandValidator _v = new();

    [Fact]
    public void Invalid_when_email_bad()
    {
        var cmd = new CreateVendorCommand("V", "bad", "123");
        _v.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateVendorCommand("Vendor", "v@demo.io", "123");
        _v.Validate(cmd).IsValid.Should().BeTrue();
    }
}

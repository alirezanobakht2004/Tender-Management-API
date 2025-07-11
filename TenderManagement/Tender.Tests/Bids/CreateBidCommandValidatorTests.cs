using FluentAssertions;
using Tender.Application.Commands.Bids;
using Xunit;

public sealed class CreateBidCommandValidatorTests
{
    private readonly CreateBidCommandValidator _v = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateBidCommand(Guid.NewGuid(), Guid.NewGuid(), 500, "ok");
        _v.Validate(cmd).IsValid.Should().BeTrue();
    }

}

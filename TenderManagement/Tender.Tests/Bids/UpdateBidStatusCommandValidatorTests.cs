using FluentAssertions;
using Tender.Application.Commands.Bids;
using Xunit;

public sealed class UpdateBidStatusCommandValidatorTests
{
    private readonly UpdateBidStatusCommandValidator _v = new();

    [Fact]
    public void Invalid_when_status_is_empty_guid()
    {
        var cmd = new UpdateBidStatusCommand(Guid.NewGuid(), Guid.Empty);
        _v.Validate(cmd).IsValid.Should().BeFalse();
    }
}

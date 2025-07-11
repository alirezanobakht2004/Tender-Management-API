using FluentAssertions;
using Tender.Application.Commands.Tenders;
using Xunit;

public sealed class UpdateTenderCommandValidatorTests
{
    private readonly UpdateTenderCommandValidator _v = new();

    [Fact]
    public void Invalid_when_deadline_past()
    {
        var cmd = new UpdateTenderCommand(Guid.NewGuid(), "T", "D",
                                          DateTime.UtcNow.AddDays(-1),
                                          Guid.NewGuid(), Guid.NewGuid());
        _v.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new UpdateTenderCommand(Guid.NewGuid(), "T", "D",
                                          DateTime.UtcNow.AddDays(2),
                                          Guid.NewGuid(), Guid.NewGuid());
        _v.Validate(cmd).IsValid.Should().BeTrue();
    }
}

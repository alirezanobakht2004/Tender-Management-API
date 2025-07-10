using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using Tender.Application.Commands.Tenders;

namespace Tender.Tests.Tenders;

public sealed class CreateTenderCommandValidatorTests
{
    private readonly CreateTenderCommandValidator _validator = new();

    [Fact]
    public void Invalid_when_title_empty()
    {
        var cmd = new CreateTenderCommand(
            "",
            "desc",
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),       // ← was 1
            Guid.NewGuid(),       // ← was 1
            Guid.NewGuid());

        var result = _validator.Validate(cmd);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateTenderCommand(
            "A",
            "B",
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _validator.Validate(cmd);
        Assert.True(result.IsValid);
    }

}

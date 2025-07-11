using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Tender.Application.Commands.Bids;
using Tender.Domain.ValueObjects;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Infrastructure.Persistence;
using Tender.Tests.Util;

namespace Tender.Tests.Bids;        // new file: UpdateBidCommandHandlerTests.cs
public sealed class UpdateBidCommandHandlerTests
{
    private static readonly Guid Pending =    // give the constant a value
        Guid.Parse("41d9b6d9-fd37-4894-a63e-65892a0cfe19");

    [Fact]
    public async Task Updates_amount_and_comments()
    {
        await using var db = InMemoryDbFactory.Create();

        var tender = new Tender.Domain.Entities.Tender(
        "Title", "Desc",
        Deadline.From(DateTime.UtcNow.AddDays(3)),
        Guid.Parse("e2ae4a2e-5a74-4f2a-b8ba-1b0bfc859101"),
        Guid.Parse("a1a3f451-a73b-484e-8fcb-0c21821f4d48"),
        Guid.NewGuid());

        var bid = tender.AddBid(Guid.NewGuid(), Money.From(1000), Pending, "init");
        db.Tenders.Add(tender);
        await db.SaveChangesAsync();

        var handler = new UpdateBidCommandHandler(
                          new BidRepository(db),
                          new TenderRepository(db),
                          new UnitOfWork(db));

        await handler.Handle(
            new UpdateBidCommand(bid.Id, 1800, "revised"),
            default);

        bid.BidAmount.Value.Should().Be(1800);
        bid.Comments.Should().Be("revised");
    }
}

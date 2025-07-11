using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Tender.Domain.Entities;
using Tender.Application.Commands.Bids;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;
using Tender.Domain.ValueObjects;
using Tender.Infrastructure.Persistence;

public sealed class CreateBidCommandHandlerTests
{
    public static readonly Guid Pending =
       Guid.Parse("41d9b6d9-fd37-4894-a63e-65892a0cfe19");

    [Fact]
    public async Task Persists_bid_and_returns_id()
    {
        await using var db = InMemoryDbFactory.Create();

        var tender = new Tender.Domain.Entities.Tender(
            "T", "D",
            Deadline.From(DateTime.UtcNow.AddDays(5)),
            Guid.Parse("e2ae4a2e-5a74-4f2a-b8ba-1b0bfc859101"),
            Guid.Parse("a1a3f451-a73b-484e-8fcb-0c21821f4d48"),
            Guid.NewGuid());

        db.Tenders.Add(tender);
        await db.SaveChangesAsync();

        var handler = new CreateBidCommandHandler(
            new TenderRepository(db),
            new BidRepository(db),         
            new UnitOfWork(db));

        var id = await handler.Handle(
            new CreateBidCommand(tender.Id, Guid.NewGuid(), 1200, "first"),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        (await db.Bids.FindAsync(id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Second_call_revises_existing_bid()
    {
        await using var db = InMemoryDbFactory.Create();

        var tender = new Tender.Domain.Entities.Tender(
            "T", "D",
            Deadline.From(DateTime.UtcNow.AddDays(5)),
            Guid.Parse("e2ae4a2e-5a74-4f2a-b8ba-1b0bfc859101"),
            Guid.Parse("a1a3f451-a73b-484e-8fcb-0c21821f4d48"),
            Guid.NewGuid());

        var vendorId = Guid.NewGuid();
        tender.AddBid(vendorId, Money.From(1000), Pending, "first");

        db.Tenders.Add(tender);
        await db.SaveChangesAsync();

        var handler = new CreateBidCommandHandler(
            new TenderRepository(db),
            new BidRepository(db),
            new UnitOfWork(db));

        var idReturned = await handler.Handle(
            new CreateBidCommand(tender.Id, vendorId, 1800, "revised"),
            CancellationToken.None);

        var bid = await db.Bids.FindAsync(idReturned);

        bid!.BidAmount.Value.Should().Be(1800);
        bid.Comments.Should().Be("revised");
    }

}

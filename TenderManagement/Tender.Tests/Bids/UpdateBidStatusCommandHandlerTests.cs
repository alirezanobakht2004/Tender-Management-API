using System;
using System.Threading.Tasks;
using FluentAssertions;
using Tender.Application.Commands.Bids;
using Tender.Domain.ValueObjects;
using Tender.Infrastructure.Persistence;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;

public sealed class UpdateBidStatusCommandHandlerTests
{
    private static readonly Guid _pending = Guid.Parse("41d9b6d9-fd37-4894-a63e-65892a0cfe19");
    private static readonly Guid _approved = Guid.Parse("d7122c1f-e7e8-4476-a2e2-19a2d906f6af");

    [Fact]
    public async Task Updates_status_and_sets_UpdatedAt()
    {
        using var db = InMemoryDbFactory.Create();

        // ── Seed a tender + bid ─────────────────────────────────────────────
        var tender = new Tender.Domain.Entities.Tender(
            "Network upgrade", "LAN cabling",
            Deadline.From(DateTime.UtcNow.AddDays(10)),
            Guid.Parse("e2ae4a2e-5a74-4f2a-b8ba-1b0bfc859101"),
            Guid.Parse("a1a3f451-a73b-484e-8fcb-0c21821f4d48"),
            Guid.NewGuid());

        var bid = tender.AddBid(
            vendorId: Guid.NewGuid(),
            amount: Money.From(1800),
            statusId: _pending,
            comments: "initial");

        db.Tenders.Add(tender);
        await db.SaveChangesAsync();

        var handler = new UpdateBidStatusCommandHandler(
                          new BidRepository(db),
                          new UnitOfWork(db));

        // ── Act ──────────────────────────────────────────────────────────────
        await handler.Handle(
            new UpdateBidStatusCommand(bid.Id, _approved),
            default);

        // ── Assert ───────────────────────────────────────────────────────────
        bid.StatusId.Should().Be(_approved);
        bid.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }
}

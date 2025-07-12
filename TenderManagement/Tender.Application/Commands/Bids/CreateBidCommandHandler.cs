using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;

namespace Tender.Application.Commands.Bids;

/// <summary>
/// Creates a new bid or overwrites the vendor’s previous bid on the same tender.
/// </summary>
public sealed class CreateBidCommandHandler : IRequestHandler<CreateBidCommand, Guid>
{
    private readonly ITenderRepository _tenders;
    private readonly IBidRepository _bids;
    private readonly IStatusRepository _statuses;
    private readonly IUnitOfWork _uow;

    public CreateBidCommandHandler(
        ITenderRepository tenders,
        IBidRepository bids,
        IStatusRepository statuses,
        IUnitOfWork uow)
    {
        _tenders = tenders;
        _bids = bids;
        _statuses = statuses;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateBidCommand cmd, CancellationToken ct)
    {
        // ── Validate tender ───────────────────────────────────────────────
        var tender = await _tenders.GetByIdAsync(cmd.TenderId, ct)
                     ?? throw new KeyNotFoundException("Tender not found");

        if (tender.Deadline.Value <= DateTime.UtcNow)
            throw new InvalidOperationException("Tender deadline has passed.");

        // ── Resolve “Pending” status id each time (keeps DB-driven) ──────
        var pendingId = await _statuses.GetIdByNameAsync("Pending", ct)
                       ?? throw new InvalidOperationException("Missing 'Pending' status");

        // Normalise the (nullable) comments field
        var comments = cmd.Comments ?? string.Empty;

        // ── 1) Vendor has already bid → revise it ────────────────────────
        var existing = await _bids.GetByTenderAndVendorAsync(cmd.TenderId, cmd.VendorId, ct);

        if (existing is not null)
        {
            existing.Revise(Money.From(cmd.BidAmount), comments);
            await _uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        // ── 2) First bid from this vendor → create new ───────────────────
        var bid = tender.AddBid(
            cmd.VendorId,
            Money.From(cmd.BidAmount),
            pendingId,
            comments);

        await _bids.AddAsync(bid, ct);
        await _uow.SaveChangesAsync(ct);

        return bid.Id;
    }
}

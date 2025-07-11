using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;

namespace Tender.Application.Commands.Bids;

public sealed class CreateBidCommandHandler : IRequestHandler<CreateBidCommand, Guid>
{
    private readonly ITenderRepository _tenders;
    private readonly IBidRepository _bids;
    private readonly IUnitOfWork _uow;
    private readonly Guid _pendingStatusId;

    public CreateBidCommandHandler(ITenderRepository tenders,
                                   IBidRepository bids,
                                   IUnitOfWork uow)
    {
        _tenders = tenders;
        _bids = bids;
        _uow = uow;
        _pendingStatusId = Guid.Parse("41d9b6d9-fd37-4894-a63e-65892a0cfe19");
    }

    public async Task<Guid> Handle(CreateBidCommand cmd, CancellationToken ct)
    {
        // 1. Load tender (tracked by THIS DbContext)
        var tender = await _tenders.GetByIdAsync(cmd.TenderId, ct)
                     ?? throw new KeyNotFoundException("Tender not found");

        // 2. Compose the aggregate-root operation
        var bid = tender.AddBid(
            cmd.VendorId,
            Money.From(cmd.BidAmount),
            _pendingStatusId,          // “Pending”
            cmd.Comments);

        await _bids.AddAsync(bid, ct);   

        // 4. Commit once
        await _uow.SaveChangesAsync(ct);

        return bid.Id;
    }

}

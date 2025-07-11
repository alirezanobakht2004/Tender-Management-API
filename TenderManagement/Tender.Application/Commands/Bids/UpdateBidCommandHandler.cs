using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Tender.Application.Commands.Bids;

public sealed class UpdateBidCommandHandler : IRequestHandler<UpdateBidCommand>
{
    private readonly IBidRepository _bids;
    private readonly ITenderRepository _tenders;
    private readonly IUnitOfWork _uow;

    public UpdateBidCommandHandler(IBidRepository bids,
                                   ITenderRepository tenders,
                                   IUnitOfWork uow)
    {
        _bids = bids;
        _tenders = tenders;
        _uow = uow;
    }

    public async Task Handle(UpdateBidCommand cmd, CancellationToken ct)
    {
        var bid = await _bids.GetByIdAsync(cmd.BidId, ct)
                  ?? throw new KeyNotFoundException("Bid not found");

        var tender = await _tenders.GetByIdAsync(bid.TenderId, ct)
                     ?? throw new KeyNotFoundException("Tender not found");

        if (tender.Deadline.Value <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot update bid after tender deadline");

        bid.Update(Money.From(cmd.BidAmount), cmd.Comments);

       
        await _uow.SaveChangesAsync(ct);
    }
}
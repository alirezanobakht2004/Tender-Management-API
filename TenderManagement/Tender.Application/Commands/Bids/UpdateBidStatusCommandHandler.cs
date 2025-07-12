using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Contracts;

namespace Tender.Application.Commands.Bids;

public sealed class UpdateBidStatusCommandHandler
        : IRequestHandler<UpdateBidStatusCommand>
{
    private readonly IBidRepository _bids;
    private readonly IUnitOfWork _uow;

    public UpdateBidStatusCommandHandler(IBidRepository bids, IUnitOfWork uow)
    {
        _bids = bids;
        _uow = uow;
    }

    public async Task Handle(UpdateBidStatusCommand cmd, CancellationToken ct)
    {
        var bid = await _bids.GetByIdAsync(cmd.BidId, ct)
                  ?? throw new KeyNotFoundException("Bid not found");

        bid.SetStatus(cmd.StatusId);      // domain method → raises concurrency token internally
        await _uow.SaveChangesAsync(ct);
    }
}


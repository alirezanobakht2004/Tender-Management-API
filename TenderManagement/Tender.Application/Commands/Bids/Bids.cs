using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Tender.Application.Commands.Bids;

public sealed record CreateBidCommand(
    Guid TenderId,
    Guid VendorId,
    decimal BidAmount,
    string Comments) : IRequest<Guid>;

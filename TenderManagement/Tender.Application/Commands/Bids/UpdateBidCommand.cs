using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace Tender.Application.Commands.Bids;

public sealed record UpdateBidCommand(
    Guid BidId,
    decimal BidAmount,
    string Comments) : IRequest;
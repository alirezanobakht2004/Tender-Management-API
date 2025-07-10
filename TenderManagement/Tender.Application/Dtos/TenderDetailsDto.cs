using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed record TenderDetailsDto(
    Guid Id,
    string Title,
    string Description,
    DateTime DeadlineUtc,
    CategoryDto Category,
    StatusDto Status,
    IReadOnlyCollection<BidSummaryDto> Bids);

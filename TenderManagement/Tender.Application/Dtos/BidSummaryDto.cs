using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed record BidSummaryDto(
    Guid Id,
    decimal Amount,
    DateTime SubmittedAtUtc,
    VendorDto Vendor,
    StatusDto Status);


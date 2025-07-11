using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed record VendorDetailsDto(
    Guid Id,
    string Name,
    string ContactEmail,
    string Phone,
    IReadOnlyCollection<BidInfoDto> Bids);
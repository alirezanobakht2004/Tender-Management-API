using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed record VendorListItemDto(
    Guid Id,
    string Name,
    string ContactEmail,
    string Phone,
    int? BidCount);      // null when caller does not ask for the summary

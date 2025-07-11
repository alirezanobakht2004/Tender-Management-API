using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Vendors;

public sealed record GetVendorsQuery(bool IncludeBidSummary)
    : IRequest<IReadOnlyCollection<VendorListItemDto>>;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Vendors;

public sealed class GetVendorsQueryHandler
    : IRequestHandler<GetVendorsQuery, IReadOnlyCollection<VendorListItemDto>>
{
    private readonly IVendorListQuery _query;
    public GetVendorsQueryHandler(IVendorListQuery query) => _query = query;

    public Task<IReadOnlyCollection<VendorListItemDto>> Handle(
        GetVendorsQuery request, CancellationToken ct) =>
        _query.ExecuteAsync(request.IncludeBidSummary, ct);
}

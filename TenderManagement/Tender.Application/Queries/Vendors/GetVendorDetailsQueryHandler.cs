using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Vendors;

public sealed class GetVendorDetailsQueryHandler
    : IRequestHandler<GetVendorDetailsQuery, VendorDetailsDto?>
{
    private readonly IVendorDetailsQuery _query;
    public GetVendorDetailsQueryHandler(IVendorDetailsQuery query) => _query = query;

    public Task<VendorDetailsDto?> Handle(GetVendorDetailsQuery req, CancellationToken ct) =>
        _query.ExecuteAsync(req.Id, ct);
}

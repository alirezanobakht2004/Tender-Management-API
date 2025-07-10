using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Tender.Application/Queries/Tenders/GetTenderListQueryHandler.cs
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tender.Application.Dtos;
using Tender.Application.Queries.Tenders.List;

namespace Tender.Application.Queries.Tenders;

public sealed class GetTenderListQueryHandler
    : IRequestHandler<GetTenderListQuery, IReadOnlyCollection<TenderSummaryDto>>
{
    private readonly ITenderListQuery _query;

    public GetTenderListQueryHandler(ITenderListQuery query) => _query = query;

    public Task<IReadOnlyCollection<TenderSummaryDto>> Handle(
        GetTenderListQuery request, CancellationToken ct) => _query.ExecuteAsync(ct);
}

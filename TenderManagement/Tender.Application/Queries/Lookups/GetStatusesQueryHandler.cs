using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Lookups;

public sealed class GetStatusesQueryHandler
    : IRequestHandler<GetStatusesQuery, IReadOnlyCollection<LookupItemDto>>
{
    private readonly IStatusListQuery _query;
    public GetStatusesQueryHandler(IStatusListQuery query) => _query = query;

    public Task<IReadOnlyCollection<LookupItemDto>> Handle(
        GetStatusesQuery request,
        CancellationToken cancellationToken) =>
        _query.ExecuteAsync(cancellationToken);
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Lookups;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<LookupItemDto>>
{
    private readonly ICategoryListQuery _query;
    public GetCategoriesQueryHandler(ICategoryListQuery query) => _query = query;

    public Task<IReadOnlyCollection<LookupItemDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken) =>
        _query.ExecuteAsync(cancellationToken);
}

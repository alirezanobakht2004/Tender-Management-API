// Tender.Application/Queries/Tenders/GetTenderDetailsQueryHandler.cs
using MediatR;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Tenders;

public sealed class GetTenderDetailsQueryHandler
    : IRequestHandler<GetTenderDetailsQuery, TenderDetailsDto?>
{
    private readonly IGetTenderWithBidsQuery _query;   // 👈 now the Application interface

    public GetTenderDetailsQueryHandler(IGetTenderWithBidsQuery query) => _query = query;

    public Task<TenderDetailsDto?> Handle(GetTenderDetailsQuery request, CancellationToken ct) =>
        _query.ExecuteAsync(request.Id, ct);
}

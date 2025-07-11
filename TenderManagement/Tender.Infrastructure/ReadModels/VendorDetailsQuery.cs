using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Tender.Application.Dtos;
using Tender.Application.Queries.Vendors;

namespace Tender.Infrastructure.ReadModels;

public sealed class VendorDetailsQuery : IVendorDetailsQuery
{
    private readonly IDbConnection _db;
    public VendorDetailsQuery(IDbConnection db) => _db = db;

    private const string Sql = """
        SELECT  v.Id               AS VendorId,
                v.Name,
                v.ContactEmail,
                v.Phone,
                b.Id               AS BidId,
                b.BidAmount,
                b.SubmittedAt,
                t.Id               AS TenderId,
                t.Title            AS TenderTitle,
                s.Id               AS StatusId,
                s.Name             AS StatusName
        FROM    Vendors v
        LEFT JOIN Bids b      ON b.VendorId = v.Id
        LEFT JOIN Tenders t   ON t.Id = b.TenderId
        LEFT JOIN Statuses s  ON s.Id = b.StatusId
        WHERE   v.Id = @Id;
        """;

    public async Task<VendorDetailsDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var lookup = new Dictionary<Guid, VendorDetailsDto>();

        await _db.QueryAsync<dynamic>(
            new CommandDefinition(Sql, new { Id = id }, cancellationToken: ct))
            .ContinueWith(t =>
            {
                foreach (var r in t.Result)
                {
                    if (!lookup.TryGetValue(id, out var dto))
                    {
                        dto = new VendorDetailsDto(
                            id,
                            (string)r.Name,
                            (string)r.ContactEmail,
                            (string)r.Phone,
                            new List<BidInfoDto>());
                        lookup.Add(id, dto);
                    }

                    if (r.BidId != null)
                    {
                        ((List<BidInfoDto>)dto.Bids).Add(new BidInfoDto(
                            (Guid)r.BidId,
                            (decimal)r.BidAmount,
                            (DateTime)r.SubmittedAt,
                            (Guid)r.TenderId,
                            (string)r.TenderTitle,
                            new StatusDto((Guid)r.StatusId, (string)r.StatusName)));
                    }
                }
            }, ct);

        return lookup.Values.SingleOrDefault();
    }
}

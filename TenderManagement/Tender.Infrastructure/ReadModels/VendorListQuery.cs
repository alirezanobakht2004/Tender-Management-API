using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Tender.Application.Dtos;
using Tender.Application.Queries.Vendors;

namespace Tender.Infrastructure.ReadModels;

public sealed class VendorListQuery : IVendorListQuery
{
    private readonly IDbConnection _db;
    public VendorListQuery(IDbConnection db) => _db = db;

    private const string BaseSql = """
        SELECT  v.Id,
                v.Name,
                v.ContactEmail,
                v.Phone
        FROM    Vendors v
        ORDER BY v.Name
        """;

    private const string SummarySql = """
        SELECT  v.Id,
                v.Name,
                v.ContactEmail,
                v.Phone,
                COUNT(b.Id) AS BidCount
        FROM    Vendors v
        LEFT JOIN Bids b ON b.VendorId = v.Id
        GROUP BY v.Id, v.Name, v.ContactEmail, v.Phone
        ORDER BY v.Name
        """;

    public async Task<IReadOnlyCollection<VendorListItemDto>> ExecuteAsync(
        bool includeSummary, CancellationToken ct = default)
    {
        var rows = await _db.QueryAsync(
            new CommandDefinition(includeSummary ? SummarySql : BaseSql,
                                  cancellationToken: ct));

        var list = new List<VendorListItemDto>();
        foreach (var r in rows)
        {
            list.Add(new VendorListItemDto(
                (Guid)r.Id,
                (string)r.Name,
                (string)r.ContactEmail,
                (string)r.Phone,
                includeSummary ? (int)(long)r.BidCount : null));
        }

        return list;
    }
}

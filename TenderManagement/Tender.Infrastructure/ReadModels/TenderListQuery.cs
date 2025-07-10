using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Tender.Application.Dtos;
using Tender.Application.Queries.Tenders.List;

namespace Tender.Infrastructure.ReadModels;

public sealed class TenderListQuery : ITenderListQuery
{
    private readonly IDbConnection _db;
    public TenderListQuery(IDbConnection db) => _db = db;

    private const string Sql = """
        SELECT  t.Id,
                t.Title,
                t.Description,
                t.Deadline        AS DeadlineUtc,
                c.Id              AS CategoryId,
                c.Name            AS CategoryName,
                s.Id              AS StatusId,
                s.Name            AS StatusName
        FROM    Tenders t
        JOIN    Categories c ON c.Id = t.CategoryId
        JOIN    Statuses   s ON s.Id = t.StatusId
        ORDER BY t.Deadline
        """;

    public async Task<IReadOnlyCollection<TenderSummaryDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var rows = await _db.QueryAsync(
            new CommandDefinition(Sql, cancellationToken: ct));

        var list = new List<TenderSummaryDto>();
        foreach (var r in rows)
        {
            list.Add(new TenderSummaryDto(
                (Guid)r.Id,
                (string)r.Title,
                (string)r.Description,
                (DateTime)r.DeadlineUtc,
                new CategoryDto((Guid)r.CategoryId, (string)r.CategoryName),
                new StatusDto((Guid)r.StatusId, (string)r.StatusName)));
        }

        return list;
    }

}

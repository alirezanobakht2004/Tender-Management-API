using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Tender.Application.Dtos;
using Tender.Application.Queries.Lookups;

namespace Tender.Infrastructure.ReadModels;

public sealed class StatusListQuery : IStatusListQuery
{
    private readonly IDbConnection _db;
    public StatusListQuery(IDbConnection db) => _db = db;

    private const string Sql = """
        SELECT Id, Name
        FROM   Statuses
        ORDER  BY Name
        """;

    public async Task<IReadOnlyCollection<LookupItemDto>> ExecuteAsync(
        CancellationToken ct = default)
    {
        var rows = await _db.QueryAsync(
            new CommandDefinition(Sql, cancellationToken: ct));

        var list = new List<LookupItemDto>();
        foreach (var r in rows)
            list.Add(new LookupItemDto((Guid)r.Id, (string)r.Name));

        return list;
    }
}

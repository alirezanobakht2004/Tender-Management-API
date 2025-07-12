using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Tender.Application.Dtos;
using Tender.Application.Queries.Tenders;   

namespace Tender.Infrastructure.ReadModels;


public sealed class GetTenderWithBidsQueryObject : IGetTenderWithBidsQuery
{
    private readonly IDbConnection _db;

    public GetTenderWithBidsQueryObject(IDbConnection db) => _db = db;

    public async Task<TenderDetailsDto?> ExecuteAsync(Guid id, CancellationToken ct)
    {
        const string sql = """
            SELECT
                t.Id                AS TenderId,
                t.Title             AS TenderTitle,
                t.Description       AS TenderDescription,
                t.Deadline          AS TenderDeadline,
                c.Id                AS CategoryId,
                c.Name              AS CategoryName,
                ts.Id               AS TenderStatusId,
                ts.Name             AS TenderStatusName,
                b.Id                AS BidId,
                b.BidAmount         AS BidAmount,
                b.Comments          AS BidComments,
                b.SubmittedAt       AS BidSubmittedAt,
                v.Id                AS VendorId,
                v.Name              AS VendorName,
                bs.Id               AS BidStatusId,
                bs.Name             AS BidStatusName
            FROM Tenders t
            JOIN Categories c     ON c.Id  = t.CategoryId
            JOIN Statuses   ts    ON ts.Id = t.StatusId
            LEFT JOIN Bids  b     ON b.TenderId = t.Id
            LEFT JOIN Vendors v   ON v.Id = b.VendorId
            LEFT JOIN Statuses bs ON bs.Id = b.StatusId
            WHERE t.Id = @Id;
            """;

        var lookup = new Dictionary<Guid, TenderDetailsDto>();

        await _db.QueryAsync<dynamic>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct))
            .ContinueWith(t =>
            {
                foreach (var row in t.Result)
                {
                    var tenderId = (Guid)row.TenderId;

                    if (!lookup.TryGetValue(tenderId, out var tenderDto))
                    {
                        tenderDto = new TenderDetailsDto(
                            tenderId,
                            (string)row.TenderTitle,
                            (string)row.TenderDescription,
                            (DateTime)row.TenderDeadline,
                            new CategoryDto((Guid)row.CategoryId, (string)row.CategoryName),
                            new StatusDto((Guid)row.TenderStatusId, (string)row.TenderStatusName),
                            new List<BidSummaryDto>());
                        lookup.Add(tenderId, tenderDto);
                    }

                    if (row.BidId != null)
                    {
                        var bids = (List<BidSummaryDto>)tenderDto.Bids;
                        bids.Add(new BidSummaryDto(
                            (Guid)row.BidId,
                            (decimal)row.BidAmount,
                            (DateTime)row.BidSubmittedAt,
                            (string)row.BidComments ?? string.Empty,
                            new VendorDto((Guid)row.VendorId, (string)row.VendorName),
                            new StatusDto((Guid)row.BidStatusId, (string)row.BidStatusName)));
                    }
                }
            }, ct);

        return lookup.Values.SingleOrDefault();
    }
}

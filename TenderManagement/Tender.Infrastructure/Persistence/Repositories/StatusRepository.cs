using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class StatusRepository : IStatusRepository
{
    private readonly TenderDbContext _db;

    public StatusRepository(TenderDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _db.Statuses.AnyAsync(s => s.Id == id, ct);
}

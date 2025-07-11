using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly TenderDbContext _db;

    public CategoryRepository(TenderDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.AnyAsync(c => c.Id == id, ct);
}

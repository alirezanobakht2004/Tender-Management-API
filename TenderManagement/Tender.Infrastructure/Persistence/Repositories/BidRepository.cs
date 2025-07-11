using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class BidRepository : IBidRepository
{
    private readonly TenderDbContext _db;
    public BidRepository(TenderDbContext db) => _db = db;

    public Task AddAsync(Bid entity, CancellationToken ct = default)
    {
        _db.Bids.Add(entity);          
        return Task.CompletedTask;
    }

    public Task<Bid?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Bids.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task DeleteAsync(Bid entity, CancellationToken ct = default)
    {
        _db.Bids.Remove(entity);
        return Task.CompletedTask;
    }
    public Task<Bid?> GetByTenderAndVendorAsync(Guid tenderId,
                                            Guid vendorId,
                                            CancellationToken ct = default) =>
    _db.Bids.FirstOrDefaultAsync(b =>
            b.TenderId == tenderId && b.VendorId == vendorId, ct);
}

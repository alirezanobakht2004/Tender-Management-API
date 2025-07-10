using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class BidRepository : IBidRepository
{
    private readonly TenderDbContext _context;

    public BidRepository(TenderDbContext context) => _context = context;

    public async Task AddAsync(Bid entity, CancellationToken cancellationToken = default) =>
        await _context.Bids.AddAsync(entity, cancellationToken);

    public async Task<Bid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Bids.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task DeleteAsync(Bid entity, CancellationToken cancellationToken = default)
    {
        _context.Bids.Remove(entity);
        return Task.CompletedTask;
    }
}


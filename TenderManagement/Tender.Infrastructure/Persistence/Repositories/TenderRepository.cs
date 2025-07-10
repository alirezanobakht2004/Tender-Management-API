using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using TenderEntity = Tender.Domain.Entities.Tender;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class TenderRepository : ITenderRepository
{
    private readonly TenderDbContext _context;

    public TenderRepository(TenderDbContext context) => _context = context;

    public async Task AddAsync(TenderEntity entity, CancellationToken cancellationToken = default) =>
        await _context.Tenders.AddAsync(entity, cancellationToken);

    public async Task<TenderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tenders.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task DeleteAsync(TenderEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Tenders.Remove(entity);
        return Task.CompletedTask;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class VendorRepository : IVendorRepository
{
    private readonly TenderDbContext _context;

    public VendorRepository(TenderDbContext context) => _context = context;

    public async Task AddAsync(Vendor entity, CancellationToken cancellationToken = default) =>
        await _context.Vendors.AddAsync(entity, cancellationToken);

    public async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task DeleteAsync(Vendor entity, CancellationToken cancellationToken = default)
    {
        _context.Vendors.Remove(entity);
        return Task.CompletedTask;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tender.Domain.Contracts;


namespace Tender.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TenderDbContext _context;

    public UnitOfWork(TenderDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

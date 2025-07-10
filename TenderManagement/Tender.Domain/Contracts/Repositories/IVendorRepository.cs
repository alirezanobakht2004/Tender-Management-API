using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tender.Domain.Entities;

namespace Tender.Domain.Contracts.Repositories;

public interface IVendorRepository
{
    Task AddAsync(Vendor entity, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Vendor entity, CancellationToken cancellationToken = default);
}

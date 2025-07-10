using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tender.Domain.Entities;

namespace Tender.Domain.Contracts.Repositories;

public interface IBidRepository
{
    Task AddAsync(Bid entity, CancellationToken cancellationToken = default);
    Task<Bid?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Bid entity, CancellationToken cancellationToken = default);
}

using System;
using System.Threading;
using System.Threading.Tasks;
using TenderEntity = Tender.Domain.Entities.Tender;   

namespace Tender.Domain.Contracts.Repositories;

public interface ITenderRepository
{
    Task AddAsync(TenderEntity entity, CancellationToken cancellationToken = default);
    Task<TenderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(TenderEntity entity, CancellationToken cancellationToken = default);
}

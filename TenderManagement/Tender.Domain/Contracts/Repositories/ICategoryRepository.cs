using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tender.Domain.Contracts.Repositories;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

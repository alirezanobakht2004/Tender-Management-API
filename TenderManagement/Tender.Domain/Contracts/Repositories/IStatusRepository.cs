using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tender.Domain.Contracts.Repositories;

public interface IStatusRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<Guid?> GetIdByNameAsync(string name, CancellationToken ct = default);
}

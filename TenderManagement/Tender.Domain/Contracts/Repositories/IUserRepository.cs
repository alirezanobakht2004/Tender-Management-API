using System;
using System.Threading;
using System.Threading.Tasks;
using Tender.Domain.Entities;

namespace Tender.Domain.Contracts.Repositories;

public interface IUserRepository
{
    Task AddAsync(User entity, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

}
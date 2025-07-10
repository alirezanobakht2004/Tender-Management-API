using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly TenderDbContext _db;
    public UserRepository(TenderDbContext db) => _db = db;

    public async Task AddAsync(User entity, CancellationToken ct = default) =>
        await _db.Users.AddAsync(entity, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
}

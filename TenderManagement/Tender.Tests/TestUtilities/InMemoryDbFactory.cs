using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Tender.Infrastructure.Persistence;

namespace Tender.Tests.Util;

internal static class InMemoryDbFactory
{
    public static TenderDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TenderDbContext>()
            .UseInMemoryDatabase($"tender-tests-{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        var db = new TenderDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}

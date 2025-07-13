using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Tender.Infrastructure.Persistence;

public sealed class TenderDbContextFactory : IDesignTimeDbContextFactory<TenderDbContext>
{
    public TenderDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TenderDbContext>()
            .UseSqlServer(args.FirstOrDefault()
               ?? "Server=.;Database=TenderDb;Trusted_Connection=True;Trust Server Certificate=True;")
            .Options;

        return new TenderDbContext(options);
    }
}

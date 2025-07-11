using System.Threading.Tasks;
using Tender.Application.Commands.Vendors;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;
using FluentAssertions;
using Tender.Infrastructure.Persistence;

public sealed class CreateVendorCommandHandlerTests
{
    [Fact]
    public async Task Persists_vendor_and_returns_id()
    {
        using var db = InMemoryDbFactory.Create();
        var repo = new VendorRepository(db);
        var uow = new UnitOfWork(db);
        var h = new CreateVendorCommandHandler(repo, uow);

        var id = await h.Handle(
                 new CreateVendorCommand("V", "v@demo.io", "123"), default);

        id.Should().NotBe(Guid.Empty);
        (await db.Vendors.FindAsync(id)).Should().NotBeNull();
    }
}

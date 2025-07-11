using Microsoft.EntityFrameworkCore;
using Tender.Application.Commands.Tenders;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Infrastructure.Persistence;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;
using NSubstitute;
using System.Threading.Tasks;
using System;

public sealed class UpdateTenderCommandHandlerTests
{
    [Fact]
    public async Task Updates_fields()
    {
        using var db = InMemoryDbFactory.Create();
        var tender = new Tender.Domain.Entities.Tender(
            "Old", "OldDesc",
            Tender.Domain.ValueObjects.Deadline.From(DateTime.UtcNow.AddDays(3)),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        db.Tenders.Add(tender);
        db.SaveChanges();

        ITenderRepository repo = new TenderRepository(db);
        IUnitOfWork uow = new UnitOfWork(db);

        // --- Add these mocks ---
        var categoryRepo = Substitute.For<ICategoryRepository>();
        var statusRepo = Substitute.For<IStatusRepository>();
        categoryRepo.ExistsAsync(Arg.Any<Guid>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(true);
        statusRepo.ExistsAsync(Arg.Any<Guid>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(true);

        var handler = new UpdateTenderCommandHandler(repo, uow, categoryRepo, statusRepo);

        var cmd = new UpdateTenderCommand(
            tender.Id, "New", "NewDesc",
            DateTime.UtcNow.AddDays(5),
            Guid.NewGuid(), Guid.NewGuid());

        await handler.Handle(cmd, default);

        var updated = await db.Tenders.FirstAsync(t => t.Id == tender.Id);
        Assert.Equal("New", updated.Title);
        Assert.Equal("NewDesc", updated.Description);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tender.Application.Commands.Tenders;
using Tender.Infrastructure.Persistence;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Tests.Util;
using Xunit;

public sealed class DeleteTenderCommandHandlerTests
{
    [Fact]
    public async Task Removes_entity()
    {
        using var db = InMemoryDbFactory.Create();
        var tender = new Tender.Domain.Entities.Tender(
            "T1", "D",
            Tender.Domain.ValueObjects.Deadline.From(DateTime.UtcNow.AddDays(3)),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        db.Tenders.Add(tender);
        db.SaveChanges();

        var handler = new DeleteTenderCommandHandler(
            new TenderRepository(db), new UnitOfWork(db));

        await handler.Handle(new DeleteTenderCommand(tender.Id), default);

        Assert.Null(await db.Tenders.FirstOrDefaultAsync(t => t.Id == tender.Id));
    }
}

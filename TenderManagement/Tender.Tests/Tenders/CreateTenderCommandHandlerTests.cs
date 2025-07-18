﻿using Tender.Application.Commands.Tenders;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using Tender.Infrastructure.Persistence.Repositories;
using Tender.Infrastructure.Persistence;
using Moq; 


namespace Tender.Tests.Tenders;

public sealed class CreateTenderCommandHandlerTests
{
    private readonly ITenderRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly TenderDbContext _db;
    private readonly CreateTenderCommandHandler _handler;

    public CreateTenderCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<TenderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Use unique DB per test class
            .Options;

        _db = new TenderDbContext(options);
        _repo = new TenderRepository(_db);
        _uow = new UnitOfWork(_db);

        // Mock ICategoryRepository and IStatusRepository for tests
        var catRepo = new Mock<ICategoryRepository>();
        var statusRepo = new Mock<IStatusRepository>();
        catRepo.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(true);
        statusRepo.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(true);

        _handler = new CreateTenderCommandHandler(_repo, _uow, catRepo.Object, statusRepo.Object);
    }

    // Tender.Tests/CreateTenderCommandHandlerTests.cs  ❱ Fix constructor args
    [Fact]
    public async Task Persists_tender()
    {
        var cmd = new CreateTenderCommand(
            "T",
            "D",
            DateTime.UtcNow.AddDays(2),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var id = await _handler.Handle(cmd, CancellationToken.None);

        var tender = await _db.Tenders.FindAsync(id);
        Assert.NotNull(tender);
        Assert.Equal("T", tender!.Title);
    }

}

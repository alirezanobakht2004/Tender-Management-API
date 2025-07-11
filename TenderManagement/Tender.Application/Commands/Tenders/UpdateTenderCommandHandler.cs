using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;
using FluentValidation; // For ValidationException

namespace Tender.Application.Commands.Tenders;

public sealed class UpdateTenderCommandHandler : IRequestHandler<UpdateTenderCommand>
{
    private readonly ITenderRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IStatusRepository _statusRepo;
    private readonly IUnitOfWork _uow;

    public UpdateTenderCommandHandler(
        ITenderRepository repo,
        IUnitOfWork uow,
        ICategoryRepository categoryRepo,
        IStatusRepository statusRepo)
    {
        _repo = repo;
        _uow = uow;
        _categoryRepo = categoryRepo;
        _statusRepo = statusRepo;
    }

    public async Task Handle(UpdateTenderCommand cmd, CancellationToken ct)
    {
        // 1. Tender existence check FIRST!
        var tender = await _repo.GetByIdAsync(cmd.Id, ct);
        if (tender == null)
            throw new KeyNotFoundException("Tender not found");

        // 2. Category existence
        var categoryExists = await _categoryRepo.ExistsAsync(cmd.CategoryId, ct);
        if (!categoryExists)
            throw new ValidationException("Invalid CategoryId.");

        // 3. Status existence
        var statusExists = await _statusRepo.ExistsAsync(cmd.StatusId, ct);
        if (!statusExists)
            throw new ValidationException("Invalid StatusId.");

        // 4. Perform update
        tender.Update(
            cmd.Title,
            cmd.Description,
            Deadline.From(cmd.DeadlineUtc),
            cmd.CategoryId,
            cmd.StatusId);

        await _uow.SaveChangesAsync(ct);
    }

}

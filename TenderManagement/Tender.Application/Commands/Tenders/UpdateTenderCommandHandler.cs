using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;

namespace Tender.Application.Commands.Tenders;

public sealed class UpdateTenderCommandHandler : IRequestHandler<UpdateTenderCommand>
{
    private readonly ITenderRepository _repo;
    private readonly IUnitOfWork _uow;

    public UpdateTenderCommandHandler(ITenderRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task Handle(UpdateTenderCommand cmd, CancellationToken ct)
    {
        var tender = await _repo.GetByIdAsync(cmd.Id, ct)
                     ?? throw new KeyNotFoundException("Tender not found");

        tender.Update(
            cmd.Title,
            cmd.Description,
            Deadline.From(cmd.DeadlineUtc),
            cmd.CategoryId,
            cmd.StatusId);

        await _uow.SaveChangesAsync(ct);
    }
}


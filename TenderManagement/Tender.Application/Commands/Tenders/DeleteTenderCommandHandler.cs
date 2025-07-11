using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;

namespace Tender.Application.Commands.Tenders;

public sealed class DeleteTenderCommandHandler : IRequestHandler<DeleteTenderCommand>
{
    private readonly ITenderRepository _repo;
    private readonly IUnitOfWork _uow;

    public DeleteTenderCommandHandler(ITenderRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task Handle(DeleteTenderCommand cmd, CancellationToken ct)
    {
        var tender = await _repo.GetByIdAsync(cmd.Id, ct);
        if (tender is null)
            return; 

        await _repo.DeleteAsync(tender, ct);
        await _uow.SaveChangesAsync(ct);
    }
}


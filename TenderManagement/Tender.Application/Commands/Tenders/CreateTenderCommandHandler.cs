using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;
using TenderEntity = Tender.Domain.Entities.Tender;

namespace Tender.Application.Commands.Tenders;

public sealed class CreateTenderCommandHandler : IRequestHandler<CreateTenderCommand, Guid>
{
    private readonly ITenderRepository _tenderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenderCommandHandler(
        ITenderRepository tenderRepository,
        IUnitOfWork unitOfWork)
    {
        _tenderRepository = tenderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTenderCommand request, CancellationToken cancellationToken)
    {
        var tender = new TenderEntity(
            request.Title,
            request.Description,
            Deadline.From(request.DeadlineUtc),
            request.CategoryId,
            request.StatusId,
            request.CreatedByUserId);

        await _tenderRepository.AddAsync(tender, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tender.Id;
    }
}

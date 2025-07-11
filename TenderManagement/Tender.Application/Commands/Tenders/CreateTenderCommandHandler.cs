using FluentValidation;
using MediatR;
using Tender.Application.Commands.Tenders;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.ValueObjects;

public sealed class CreateTenderCommandHandler : IRequestHandler<CreateTenderCommand, Guid>
{
    private readonly ITenderRepository _tenderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IStatusRepository _statusRepository;

    public CreateTenderCommandHandler(
        ITenderRepository tenderRepository,
        IUnitOfWork unitOfWork,
        ICategoryRepository categoryRepository,
        IStatusRepository statusRepository)
    {
        _tenderRepository = tenderRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _statusRepository = statusRepository;
    }

    public async Task<Guid> Handle(CreateTenderCommand request, CancellationToken cancellationToken)
    {
        // Ensure Category exists
        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new ValidationException("CategoryId does not exist");

        // Ensure Status exists
        var statusExists = await _statusRepository.ExistsAsync(request.StatusId, cancellationToken);
        if (!statusExists)
            throw new ValidationException("StatusId does not exist");

        var tender = new Tender.Domain.Entities.Tender(
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Tender.Application.Commands.Bids;

public sealed record UpdateBidStatusCommand(
        Guid BidId,
        Guid NewStatusId) : IRequest;


public sealed class UpdateBidStatusCommandValidator
        : AbstractValidator<UpdateBidStatusCommand>
{
    public UpdateBidStatusCommandValidator()
    {
        RuleFor(c => c.BidId).NotEmpty();
        RuleFor(c => c.NewStatusId).NotEmpty();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace Tender.Application.Commands.Bids;

public sealed class UpdateBidCommandValidator : AbstractValidator<UpdateBidCommand>
{
    public UpdateBidCommandValidator()
    {
        RuleFor(x => x.BidId).NotEmpty();
        RuleFor(x => x.BidAmount).GreaterThan(0);
        RuleFor(x => x.Comments).MaximumLength(500);
    }
}
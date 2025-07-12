using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Tender.Application.Commands.Bids;

public sealed class CreateBidCommandValidator : AbstractValidator<CreateBidCommand>
{
    public CreateBidCommandValidator()
    {
        RuleFor(b => b.TenderId).NotEmpty();
        RuleFor(b => b.VendorId).NotEmpty();
        RuleFor(b => b.BidAmount).GreaterThan(0);
        RuleFor(b => b.Comments)
            .MaximumLength(500)
            .When(b => !string.IsNullOrWhiteSpace(b.Comments));
    }
}

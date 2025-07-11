using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Tender.Application.Commands.Tenders;

public sealed class UpdateTenderCommandValidator : AbstractValidator<UpdateTenderCommand>
{
    public UpdateTenderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.DeadlineUtc)
            .Must(d => d > DateTime.UtcNow).WithMessage("Deadline must be in the future.");
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.StatusId).NotEmpty();
    }
}

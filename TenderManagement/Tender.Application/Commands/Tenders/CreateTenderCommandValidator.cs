using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Tender.Application.Commands.Tenders;

public sealed class CreateTenderCommandValidator : AbstractValidator<CreateTenderCommand>
{
    public CreateTenderCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.Description)
            .NotEmpty();

        RuleFor(c => c.DeadlineUtc)
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("Deadline must be in the future.");

        RuleFor(c => c.CategoryId)
            .GreaterThan(0);

        RuleFor(c => c.StatusId)
            .GreaterThan(0);
    }
}

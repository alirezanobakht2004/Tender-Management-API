using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace Tender.Application.Commands.Vendors;

public sealed class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(30);
    }
}

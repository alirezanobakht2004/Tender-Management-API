using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace Tender.Application.Commands.Vendors;

public sealed record CreateVendorCommand(
    string Name,
    string ContactEmail,
    string Phone) : IRequest<Guid>;

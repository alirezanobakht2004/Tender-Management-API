using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Tender.Application.Commands.Tenders;

public sealed record UpdateTenderCommand(
    Guid Id,
    string Title,
    string Description,
    DateTime DeadlineUtc,
    Guid CategoryId,
    Guid StatusId) : IRequest;

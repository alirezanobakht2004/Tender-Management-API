﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Tender.Application.Commands.Tenders;

public sealed record CreateTenderCommand(
    string Title,
    string Description,
    DateTime DeadlineUtc,
    Guid CategoryId,
    Guid StatusId,
    Guid CreatedByUserId) : IRequest<Guid>;

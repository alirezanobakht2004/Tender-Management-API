using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using System.Collections.Generic;
using Tender.Application.Dtos;

public sealed record GetStatusesQuery() : IRequest<IReadOnlyCollection<LookupItemDto>>;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed record TenderSummaryDto(
    Guid Id,
    string Title,
    string Description,
    DateTime DeadlineUtc,
    CategoryDto Category,
    StatusDto Status);

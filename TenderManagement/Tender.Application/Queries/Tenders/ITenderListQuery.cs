using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Tenders.List;

public interface ITenderListQuery
{
    Task<IReadOnlyCollection<TenderSummaryDto>> ExecuteAsync(CancellationToken ct = default);
}

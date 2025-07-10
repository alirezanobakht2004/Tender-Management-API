using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tender.Application.Dtos;

namespace Tender.Application.Queries.Tenders;

public interface IGetTenderWithBidsQuery
{
    Task<TenderDetailsDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

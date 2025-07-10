using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tender.Application.Dtos;

public interface IStatusListQuery
{
    Task<IReadOnlyCollection<LookupItemDto>> ExecuteAsync(CancellationToken ct = default);
}
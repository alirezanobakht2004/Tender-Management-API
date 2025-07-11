using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tender.Application.Dtos;

namespace Tender.Application.Queries.Vendors;

public interface IVendorDetailsQuery
{
    Task<VendorDetailsDto?> ExecuteAsync(Guid id, CancellationToken ct = default);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed class UpdateBidDto
{
    public decimal BidAmount { get; init; }
    public string Comments { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = Array.Empty<byte>();
}
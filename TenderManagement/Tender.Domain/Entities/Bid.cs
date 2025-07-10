using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Tender.Domain.ValueObjects;

namespace Tender.Domain.Entities;

public class Bid : BaseEntity
{
    public Guid TenderId { get; private set; }
    public Guid VendorId { get; private set; }
    public Money BidAmount { get; private set; } = null!;
    public int StatusId { get; private set; }
    public string Comments { get; private set; } = default!;
    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

    internal Bid() { }

    public Bid(Guid tenderId, Guid vendorId, Money bidAmount, int statusId, string comments)
    {
        TenderId = tenderId;
        VendorId = vendorId;
        BidAmount = bidAmount;
        StatusId = statusId;
        Comments = comments;
    }

    public void UpdateStatus(int newStatusId) { StatusId = newStatusId; MarkUpdated(); }
}
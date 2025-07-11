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
    public Guid StatusId { get; private set; }
    public string Comments { get; private set; } = default!;
    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

    internal Bid() { }

    public Bid(Guid tenderId, Guid vendorId, Money bidAmount, Guid statusId, string comments)
    {
        TenderId = tenderId;
        VendorId = vendorId;
        BidAmount = bidAmount;
        StatusId = statusId;
        Comments = comments;
    }


    public void Update(Money newAmount, string newComments)
    {
        if (newAmount is null) throw new ArgumentNullException(nameof(newAmount));
        BidAmount = newAmount;
        Comments = newComments;
        Touch();                        
    }

    public void Revise(Money newAmount, string newComments)
    {
        if (newAmount is null) throw new ArgumentNullException(nameof(newAmount));
        BidAmount = newAmount;
        Comments = newComments;
        Touch();                 
    }

    public void UpdateStatus(Guid newStatusId) { StatusId = newStatusId; MarkUpdated(); }

    public void SetStatus(Guid statusId)
    {
        if (statusId == Guid.Empty)
            throw new ArgumentException("StatusId cannot be empty", nameof(statusId));

        StatusId = statusId;
        Touch();          // ← bumps UpdatedAt via the protected helper
    }


}
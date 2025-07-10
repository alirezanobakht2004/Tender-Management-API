// Tender.cs
using System;
using System.Collections.Generic;
using Tender.Domain.ValueObjects;

namespace Tender.Domain.Entities;

public class Tender : BaseEntity
{
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public Deadline Deadline { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public int StatusId { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private readonly List<Bid> _bids = new();
    public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

    internal Tender() { }

    public Tender(string title, string description, Deadline deadline, int categoryId, int statusId, Guid createdByUserId)
    {
        Title = title;
        Description = description;
        Deadline = deadline;
        CategoryId = categoryId;
        StatusId = statusId;
        CreatedByUserId = createdByUserId;
    }

    public Bid AddBid(Guid vendorId, Money amount, int statusId, string comments)
    {
        var bid = new Bid(Id, vendorId, amount, statusId, comments);
        _bids.Add(bid);
        return bid;
    }
}
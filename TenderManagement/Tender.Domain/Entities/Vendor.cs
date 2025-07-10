using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Domain.Entities;

public class Vendor : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string ContactEmail { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public DateTime RegisteredAt { get; private set; } = DateTime.UtcNow;
    public bool IsBlacklisted { get; private set; }

    internal Vendor() { }

    public Vendor(string name, string contactEmail, string phone)
    {
        Name = name;
        ContactEmail = contactEmail;
        Phone = phone;
    }

    public void Blacklist() { IsBlacklisted = true; MarkUpdated(); }
    public void Unblacklist() { IsBlacklisted = false; MarkUpdated(); }
}
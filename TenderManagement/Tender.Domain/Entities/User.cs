using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Tender.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public Guid? VendorId { get; private set; }
    public bool IsLocked { get; private set; }

    internal User() { }

    public User(string email, string passwordHash, string role, Guid? vendorId = null)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        VendorId = vendorId;
    }

    public void Lock() { IsLocked = true; MarkUpdated(); }
    public void Unlock() { IsLocked = false; MarkUpdated(); }

    public void SetPasswordHash(string hash) => PasswordHash = hash;

}

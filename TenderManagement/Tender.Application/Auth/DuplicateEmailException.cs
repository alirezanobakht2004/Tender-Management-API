using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Auth;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"Email already registered: {email}") { }
}

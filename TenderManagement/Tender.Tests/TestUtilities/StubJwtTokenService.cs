using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tender.Application.Auth;
using Tender.Domain.Entities;

namespace Tender.Tests.Util;

internal sealed class StubJwtTokenService : IJwtTokenService
{
    public string LastUserEmail = string.Empty;
    public string Generate(User user)
    {
        LastUserEmail = user.Email;
        return "stub-jwt-token";
    }
}

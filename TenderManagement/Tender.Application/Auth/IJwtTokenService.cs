using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tender.Domain.Entities;

namespace Tender.Application.Auth;

public interface IJwtTokenService
{
    string Generate(User user);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Application.Dtos;

public sealed record LookupItemDto(Guid Id, string Name);

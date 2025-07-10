using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace Tender.Application.Auth.Dto;

public sealed record LoginQuery(string Email,
                                string Password) : IRequest<string>;   // returns JWT
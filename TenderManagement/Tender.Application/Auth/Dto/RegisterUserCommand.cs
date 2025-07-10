using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace Tender.Application.Auth.Dto;

public sealed record RegisterUserCommand(string Email,
                                         string Password,
                                         string Role) : IRequest<Guid>;
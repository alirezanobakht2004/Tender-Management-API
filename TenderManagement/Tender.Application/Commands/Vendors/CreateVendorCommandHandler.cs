using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Tender.Domain.Contracts;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Entities;

namespace Tender.Application.Commands.Vendors;

public sealed class CreateVendorCommandHandler
    : IRequestHandler<CreateVendorCommand, Guid>
{
    private readonly IVendorRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateVendorCommandHandler(IVendorRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateVendorCommand cmd, CancellationToken ct)
    {
        var vendor = new Vendor(cmd.Name, cmd.ContactEmail, cmd.Phone);
        await _repo.AddAsync(vendor, ct);
        await _uow.SaveChangesAsync(ct);
        return vendor.Id;
    }
}

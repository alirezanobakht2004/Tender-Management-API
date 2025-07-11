using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tender.Application.Dtos;
using Tender.Application.Queries.Vendors;
using Xunit;

public sealed class GetVendorDetailsQueryHandlerTests
{
    [Fact]
    public async Task Returns_null_when_not_found()
    {
        var stub = Substitute.For<IVendorDetailsQuery>();
        stub.ExecuteAsync(Arg.Any<Guid>(), default).Returns((VendorDetailsDto?)null);

        var h = new GetVendorDetailsQueryHandler(stub);
        var res = await h.Handle(new GetVendorDetailsQuery(Guid.NewGuid()), default);

        res.Should().BeNull();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tender.Application.Dtos;
using Tender.Application.Queries.Vendors;
using Xunit;

public sealed class GetVendorsQueryHandlerTests
{
    [Fact]
    public async Task Delegates_to_query_object()
    {
        var stub = Substitute.For<IVendorListQuery>();
        stub.ExecuteAsync(true, default)
            .Returns(new List<VendorListItemDto>());

        var h = new GetVendorsQueryHandler(stub);

        var result = await h.Handle(new GetVendorsQuery(true), default);

        result.Should().NotBeNull();
        await stub.Received(1).ExecuteAsync(true, default);
    }
}

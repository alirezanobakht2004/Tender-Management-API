using System.Threading.Tasks;
using Tender.Application.Queries.Tenders;
using Tender.Application.Queries.Tenders.List;
using Xunit;
using FluentAssertions;
using NSubstitute;
using System.Collections.Generic;
using Tender.Application.Dtos;

namespace Tender.Tests.Tenders;

public sealed class GetTenderListQueryHandlerTests
{
    [Fact]
    public async Task Returns_list_from_query_object()
    {
        var stub = Substitute.For<ITenderListQuery>();
        stub.ExecuteAsync(default).Returns(new List<TenderSummaryDto>());

        var handler = new GetTenderListQueryHandler(stub);
        var list = await handler.Handle(new GetTenderListQuery(), default);

        list.Should().NotBeNull();
        await stub.Received(1).ExecuteAsync(default);
    }
}

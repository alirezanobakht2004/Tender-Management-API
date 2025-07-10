using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tender.Application.Dtos;
using Tender.Application.Queries.Lookups;
using Xunit;

public sealed class GetCategoriesQueryHandlerTests
{
    [Fact]
    public async Task Returns_lookup_collection()
    {
        var stub = Substitute.For<ICategoryListQuery>();
        stub.ExecuteAsync(default).Returns(new List<LookupItemDto>());

        var h = new GetCategoriesQueryHandler(stub);
        var result = await h.Handle(new GetCategoriesQuery(), default);

        result.Should().NotBeNull();
        await stub.Received(1).ExecuteAsync(default);
    }
}

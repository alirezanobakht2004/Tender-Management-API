using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Lookups;

public sealed class LookupApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LookupApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    public class LookupItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/categories
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Categories_Endpoint_Returns_NonEmpty_List()
    {
        var resp = await _client.GetAsync("/api/categories");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await resp.Content.ReadFromJsonAsync<List<LookupItemDto>>();
        data.Should().NotBeNull();
        data!.Should().NotBeEmpty();               // at least 1 category seeded
        data.Should().OnlyContain(c => c.Id != Guid.Empty && !string.IsNullOrWhiteSpace(c.Name));
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /api/statuses
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Statuses_Endpoint_Returns_Known_Statuses()
    {
        var resp = await _client.GetAsync("/api/statuses");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await resp.Content.ReadFromJsonAsync<List<LookupItemDto>>();
        data.Should().NotBeNull();
        data!.Should().NotBeEmpty();
        data.Should().OnlyContain(s => s.Id != Guid.Empty && !string.IsNullOrWhiteSpace(s.Name));

        // Optional – assert that at least the core statuses exist
        var names = data.Select(s => s.Name).ToList();
        names.Should().Contain("Open").And.Contain("Closed").And.Contain("Pending");
    }
}
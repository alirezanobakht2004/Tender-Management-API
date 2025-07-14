using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Vendors;

public sealed class VendorApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VendorApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Can_Create_And_Get_Vendor_By_Id()
    {
        var resp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = $"Vendor {Guid.NewGuid()}",
            contactEmail = $"v{Guid.NewGuid():N}@example.com",
            phone = "123-456-7890"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await resp.Content.ReadFromJsonAsync<CreateResult>();
        created!.Id.Should().NotBeEmpty();

        var get = await _client.GetAsync($"/api/vendors/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await get.Content.ReadFromJsonAsync<VendorDetailsDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(created.Id);
        dto.Name.Should().NotBeNullOrWhiteSpace();
        dto.ContactEmail.Should().NotBeNullOrWhiteSpace();
        dto.Phone.Should().NotBeNullOrWhiteSpace();
        dto.Bids.Should().NotBeNull();
    }

    [Fact]
    public async Task List_Vendors_Returns_Vendors()
    {
        // Create at least one vendor
        var createResp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = $"Vendor {Guid.NewGuid()}",
            contactEmail = $"v{Guid.NewGuid():N}@example.com",
            phone = "222-333-4444"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var resp = await _client.GetAsync("/api/vendors");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var vendors = await resp.Content.ReadFromJsonAsync<List<VendorListItemDto>>();
        vendors.Should().NotBeNull();
        vendors!.Should().NotBeEmpty();
        foreach (var v in vendors)
        {
            v.Id.Should().NotBeEmpty();
            v.Name.Should().NotBeNullOrWhiteSpace();
            v.ContactEmail.Should().NotBeNullOrWhiteSpace();
            v.Phone.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task List_Vendors_With_Bid_Summary()
    {
        var resp = await _client.GetAsync("/api/vendors?includeBidSummary=true");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var vendors = await resp.Content.ReadFromJsonAsync<List<VendorListItemDto>>();
        vendors.Should().NotBeNull();
        foreach (var v in vendors!)
        {
            if (v.BidCount != null)
                v.BidCount.Value.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task Cannot_Create_Vendor_With_Missing_Name()
    {
        var resp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            // name missing
            contactEmail = $"v{Guid.NewGuid():N}@example.com",
            phone = "123-456-7890"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task Cannot_Create_Vendor_With_Missing_Email()
    {
        var resp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = $"Vendor {Guid.NewGuid()}",
            // contactEmail missing
            phone = "123-456-7890"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("ContactEmail");
    }

    [Fact]
    public async Task Cannot_Create_Vendor_With_Bad_Email()
    {
        var resp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = $"Vendor {Guid.NewGuid()}",
            contactEmail = "not-an-email",
            phone = "123-456-7890"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("ContactEmail");
    }

    [Fact]
    public async Task Cannot_Create_Vendor_With_Missing_Phone()
    {
        var resp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = $"Vendor {Guid.NewGuid()}",
            contactEmail = $"v{Guid.NewGuid():N}@example.com"
            // phone missing
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Phone");
    }

    [Fact]
    public async Task Get_Vendor_NotFound_For_Unknown_Id()
    {
        var resp = await _client.GetAsync($"/api/vendors/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- DTOs for deserialization ---
    public class CreateResult { public Guid Id { get; set; } }
    public class VendorListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string Phone { get; set; } = "";
        public int? BidCount { get; set; }
    }
    public class VendorDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string Phone { get; set; } = "";
        public List<BidInfoDto> Bids { get; set; } = new();
    }
    public class BidInfoDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string TenderTitle { get; set; } = "";
        public StatusDto Status { get; set; } = new();
    }
    public class StatusDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
}
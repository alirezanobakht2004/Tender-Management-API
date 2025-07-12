using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Bids;

public sealed class BidApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BidApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    // === Helpers ===
    private async Task<string> RegisterAndLogin(string role)
    {
        var email = $"{role.ToLower()}+{Guid.NewGuid()}@test.com";
        var pwd = role + "123!";
        var reg = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = pwd, role });
        reg.StatusCode.Should().Be(HttpStatusCode.Created);
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = pwd });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await login.Content.ReadFromJsonAsync<LoginResult>();
        return res!.Token;
    }

    private async Task<Guid> GetAny(string url)
    {
        var resp = await _client.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<List<LookupDto>>();
        return data!.First().Id;
    }

    private async Task<(Guid TenderId, Guid VendorId, string VendorToken, string AdminToken)> CreateTenderAndVendor()
    {
        // 1. Register Admin and create tender
        var adminToken = await RegisterAndLogin("Admin");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var catId = await GetAny("/api/categories");
        var statusId = await GetAny("/api/statuses");
        var tenderResp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Bid API Test Tender",
            description = "Test",
            deadlineUtc = DateTime.UtcNow.AddDays(10),
            categoryId = catId,
            statusId = statusId
        });
        tenderResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenderId = (await tenderResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // 2. Register Vendor and create vendor
        var vendorToken = await RegisterAndLogin("Vendor");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);
        var vendorResp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = "Vendor X",
            contactEmail = $"contact+{Guid.NewGuid()}@test.com",
            phone = "1234567890"
        });
        vendorResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var vendorId = (await vendorResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;
        return (tenderId, vendorId, vendorToken, adminToken);
    }

    // === POST /api/bids ===
    [Fact]
    public async Task Vendor_Can_Submit_New_Bid()
    {
        var (tenderId, vendorId, vendorToken, adminToken) = await CreateTenderAndVendor();

        // Login as Vendor
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);

        var resp = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId,
            vendorId,
            bidAmount = 1000,
            comments = "Initial bid"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var bidId = (await resp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // GET bid via tender details, should be present and status = "Pending"
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var tenderDetails = await _client.GetAsync($"/api/tenders/{tenderId}");
        tenderDetails.StatusCode.Should().Be(HttpStatusCode.OK);
        var tenderDto = await tenderDetails.Content.ReadFromJsonAsync<TenderDetailsDto>();
        var bid = tenderDto!.Bids.SingleOrDefault(b => b.Id == bidId);
        bid.Should().NotBeNull();
        bid!.Amount.Should().Be(1000);
        bid.Status.Should().NotBeNull();
        bid.Status.Name.Should().Be("Pending");
    }

    [Fact]
    public async Task Cannot_Submit_Bid_With_Missing_Fields()
    {
        // Login as Vendor to authorize the request
        var vendorToken = await RegisterAndLogin("Vendor");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);

        var resp = await _client.PostAsJsonAsync("/api/bids", new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("TenderId");
        problem.Errors.Should().ContainKey("VendorId");
        problem.Errors.Should().ContainKey("BidAmount");
    }

    [Fact]
    public async Task Cannot_Bid_On_Nonexistent_Tender_Or_Vendor()
    {
        // Login as Vendor to authorize the request
        var vendorToken = await RegisterAndLogin("Vendor");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);

        var resp = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId = Guid.NewGuid(),
            vendorId = Guid.NewGuid(),
            bidAmount = 999,
            comments = "no"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Submitting_Second_Bid_Overwrites_Previous()
    {
        var (tenderId, vendorId, vendorToken, adminToken) = await CreateTenderAndVendor();

        // Vendor submits first bid
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);
        var resp1 = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId,
            vendorId,
            bidAmount = 100,
            comments = "First"
        });
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Vendor submits second (should revise)
        var resp2 = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId,
            vendorId,
            bidAmount = 200,
            comments = "Revised"
        });
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);

        var bidId = (await resp2.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Fetch, amount and comment should be revised
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var tenderDetails = await _client.GetAsync($"/api/tenders/{tenderId}");
        var tenderDto = await tenderDetails.Content.ReadFromJsonAsync<TenderDetailsDto>();
        var bid = tenderDto!.Bids.Single(b => b.Id == bidId);
        bid.Amount.Should().Be(200);
        bid.Comments.Should().Be("Revised");
    }

    // === PUT /api/bids/{id}/status ===
    [Fact]
    public async Task Admin_Can_Update_Bid_Status()
    {
        var (tenderId, vendorId, vendorToken, adminToken) = await CreateTenderAndVendor();

        // Vendor creates bid
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);
        var bidResp = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId,
            vendorId,
            bidAmount = 111,
            comments = "For status"
        });
        var bidId = (await bidResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Get a statusId other than Pending
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var statuses = await _client.GetFromJsonAsync<List<LookupDto>>("/api/statuses");
        var statusId = statuses!.First(s => s.Name != "Pending").Id;

        // Update status as Admin
        var put = await _client.PutAsJsonAsync($"/api/bids/{bidId}/status", new { statusId });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Fetch and verify status updated
        var tenderDetails = await _client.GetAsync($"/api/tenders/{tenderId}");
        var tenderDto = await tenderDetails.Content.ReadFromJsonAsync<TenderDetailsDto>();
        var bid = tenderDto!.Bids.Single(b => b.Id == bidId);
        bid.Status.Id.Should().Be(statusId);
    }

    [Fact]
    public async Task Non_Admin_Cannot_Update_Bid_Status()
    {
        var (tenderId, vendorId, vendorToken, adminToken) = await CreateTenderAndVendor();

        // Vendor creates bid
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vendorToken);
        var bidResp = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId,
            vendorId,
            bidAmount = 111,
            comments = "For status"
        });
        var bidId = (await bidResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Try update as Vendor
        var statuses = await _client.GetFromJsonAsync<List<LookupDto>>("/api/statuses");
        var statusId = statuses!.First(s => s.Name != "Pending").Id;
        var put = await _client.PutAsJsonAsync($"/api/bids/{bidId}/status", new { statusId });
        put.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_Status_Of_Nonexistent_Bid_Returns_NotFound()
    {
        var adminToken = await RegisterAndLogin("Admin");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var put = await _client.PutAsJsonAsync($"/api/bids/{Guid.NewGuid()}/status", new { statusId = Guid.NewGuid() });
        put.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Status_Requires_StatusId()
    {
        var adminToken = await RegisterAndLogin("Admin");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var put = await _client.PutAsJsonAsync($"/api/bids/{Guid.NewGuid()}/status", new { });
        put.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await put.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("StatusId");
    }

    // --- Test DTOs
    public class CreateResult { public Guid Id { get; set; } }
    public class LoginResult { public string Token { get; set; } = ""; }
    public class LookupDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
    public class TenderDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DeadlineUtc { get; set; }
        public CategoryDto Category { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
        public List<BidDto> Bids { get; set; } = new();
    }
    public class CategoryDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
    public class StatusDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
    public class BidDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Comments { get; set; } = "";
        public DateTime SubmittedAtUtc { get; set; }
        public VendorDto Vendor { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
    }
    public class VendorDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
    public class ValidationProblemDetails
    {
        public string Title { get; set; } = "";
        public int? Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}

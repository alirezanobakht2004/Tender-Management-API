using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Tender.Application.Dtos;
using Xunit;

namespace Tender.Tests.Tenders;

public sealed class TenderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TenderApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetTender_By_Id_Returns_Details_With_Bids()
    {
        // --- 1. Create (register & login as Admin) ---
        var adminEmail = $"admin+{Guid.NewGuid()}@test.com";
        var adminPwd = "ApiTest123!";
        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = adminEmail,
            password = adminPwd,
            role = "Admin"
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = adminEmail,
            password = adminPwd
        });
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.Token);

        // --- 2. Create Category & Status: (Assuming seeded; else create/lookup by API)
        // We'll use whatever Category and Status IDs exist in your DB (replace with valid values as needed)
        var catId = await GetCategoryId();
        var statusId = await GetStatusId();

        // --- 3. POST Tender ---
        var tenderTitle = $"Tender {Guid.NewGuid()}";
        var tenderResp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = tenderTitle,
            description = "API test tender",
            deadlineUtc = DateTime.UtcNow.AddDays(3),    // <<<<<< USE THE CORRECT PROPERTY NAME
            categoryId = catId,
            statusId = statusId
        });
        tenderResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = tenderResp.Headers.Location;
        location.Should().NotBeNull("Location header must be set on Created response");
        var tenderId = Guid.Parse(location!.Segments.Last());


        // --- 4. POST Vendor ---
        var vendorResp = await _client.PostAsJsonAsync("/api/vendors", new
        {
            name = $"Vendor {Guid.NewGuid()}",
            contactEmail = $"contact+{Guid.NewGuid()}@test.com",
            phone = "1234567890"
        });
        vendorResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var vendorId = (await vendorResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // --- 5. POST Bid (as Vendor) ---
        var bidEmail = $"vendor+{Guid.NewGuid()}@test.com";
        var bidPwd = "Vendor123!";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = bidEmail,
            password = bidPwd,
            role = "Vendor"
        });
        var vloginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = bidEmail,
            password = bidPwd
        });
        var vlogin = await vloginResp.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vlogin!.Token);

        var bidResp = await _client.PostAsJsonAsync("/api/bids", new
        {
            tenderId,
            vendorId,
            bidAmount = 1234.56M,
            comments = "Test bid"
        });
        bidResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // --- 6. GET Tender Details ---
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.Token); // Admin again
        var getResp = await _client.GetAsync($"/api/tenders/{tenderId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var tender = await getResp.Content.ReadFromJsonAsync<TenderDetailsDto>();
        tender.Should().NotBeNull();
        tender!.Id.Should().Be(tenderId);
        tender.Title.Should().Be(tenderTitle);
        tender.Category.Should().NotBeNull();
        tender.Status.Should().NotBeNull();
        tender.Bids.Should().NotBeNull();
        tender.Bids.Should().NotBeEmpty();

        foreach (var bid in tender.Bids)
        {
            bid.Id.Should().NotBeEmpty();
            bid.Amount.Should().BeGreaterThan(0);
            bid.SubmittedAtUtc.Should().BeAfter(DateTime.UtcNow.AddDays(-2)); // Accept within a day
            bid.Vendor.Should().NotBeNull();
            bid.Vendor.Id.Should().Be(vendorId);
            bid.Vendor.Name.Should().NotBeNullOrWhiteSpace();
            bid.Status.Should().NotBeNull();
            bid.Status.Id.Should().NotBeEmpty();
            bid.Status.Name.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetTenders_Returns_List_With_Expected_Shape()
    {
        var resp = await _client.GetAsync("/api/tenders");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var tenders = await resp.Content.ReadFromJsonAsync<List<TenderSummaryDto>>();
        tenders.Should().NotBeNull();
        tenders!.Should().NotBeEmpty();
        foreach (var t in tenders)
        {
            t.Id.Should().NotBeEmpty();
            t.Title.Should().NotBeNullOrWhiteSpace();
            t.Description.Should().NotBeNull();
            t.DeadlineUtc.Should().BeAfter(DateTime.UtcNow.AddDays(-100));
            t.Category.Should().NotBeNull();
            t.Status.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Cannot_Create_Tender_If_Not_Admin()
    {
        // Register as Vendor
        var vendorEmail = $"vendor+{Guid.NewGuid()}@test.com";
        var vendorPwd = "Vendor123!";
        var reg = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = vendorEmail,
            password = vendorPwd,
            role = "Vendor"
        });
        reg.StatusCode.Should().Be(HttpStatusCode.Created);
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = vendorEmail,
            password = vendorPwd
        });
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token;
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Try to create tender as vendor
        var catId = await GetCategoryId();
        var statusId = await GetStatusId();
        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "NotAllowed",
            description = "Should fail",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = catId,
            statusId = statusId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cannot_Create_Tender_With_Missing_Fields()
    {
        // Register & login as Admin
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "ApiTest123!";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Admin"
        });
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token;
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Missing title
        var catId = await GetCategoryId();
        var statusId = await GetStatusId();
        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            description = "Missing title",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = catId,
            statusId = statusId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cannot_Create_Tender_With_Invalid_Deadline()
    {
        // Register & login as Admin
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "ApiTest123!";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Admin"
        });
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token;
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Deadline in the past
        var catId = await GetCategoryId();
        var statusId = await GetStatusId();
        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Bad deadline",
            description = "Deadline in the past",
            deadlineUtc = DateTime.UtcNow.AddDays(-5),
            categoryId = catId,
            statusId = statusId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTender_Returns_404_For_Unknown_Id()
    {
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "ApiTest123!";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Admin"
        });
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token;
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.GetAsync($"/api/tenders/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cannot_Create_Tender_With_Invalid_CategoryId()
    {
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "ApiTest123!";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Admin"
        });
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.Token;
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var statusId = await GetStatusId();
        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Bad category",
            description = "CategoryId is wrong",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = Guid.NewGuid(),
            statusId = statusId
        });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }


    // Helper to get any category/status id from lookup API
    private async Task<Guid> GetCategoryId()
    {
        var resp = await _client.GetAsync("/api/categories");
        resp.EnsureSuccessStatusCode();
        var cats = await resp.Content.ReadFromJsonAsync<List<LookupDto>>();
        return cats!.First().Id;
    }
    private async Task<Guid> GetStatusId()
    {
        var resp = await _client.GetAsync("/api/statuses");
        resp.EnsureSuccessStatusCode();
        var sts = await resp.Content.ReadFromJsonAsync<List<LookupDto>>();
        return sts!.First().Id;
    }

    // DTOs
    public class CreateResult { public Guid Id { get; set; } }
    public class LoginResult { public string Token { get; set; } = ""; }
    public class TenderDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DeadlineUtc { get; set; } // <-- MUST MATCH API DTO
        public CategoryDto Category { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
        public List<BidDto> Bids { get; set; } = new();
    }
    public class TenderSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DeadlineUtc { get; set; }  // <-- not Deadline!
        public CategoryDto Category { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
    }


    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class StatusDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class BidDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
        public VendorDto Vendor { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
    }
    public class VendorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
}

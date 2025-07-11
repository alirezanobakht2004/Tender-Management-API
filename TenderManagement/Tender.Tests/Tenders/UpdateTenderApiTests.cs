using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Tenders;

public sealed class UpdateTenderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UpdateTenderApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    private async Task<(string Token, Guid CategoryId, Guid StatusId)> RegisterAndLoginAdminAsync()
    {
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "Admin123!";

        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Admin"
        });
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var res = await login.Content.ReadFromJsonAsync<LoginResult>();
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var catId = await GetId("/api/categories");
        var statusId = await GetId("/api/statuses");

        return (res!.Token, catId, statusId);
    }

    private async Task<Guid> GetId(string url)
    {
        var resp = await _client.GetAsync(url);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await resp.Content.ReadFromJsonAsync<List<LookupDto>>();
        return data!.First().Id;
    }

    [Fact]
    public async Task Admin_Can_Update_Tender()
    {
        var (token, catId, statusId) = await RegisterAndLoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create tender
        var createResp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Initial Title",
            description = "Initial Desc",
            deadlineUtc = DateTime.UtcNow.AddDays(3),
            categoryId = catId,
            statusId = statusId
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var tid = (await createResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Update tender
        var updateResp = await _client.PutAsJsonAsync($"/api/tenders/{tid}", new
        {
            id = tid,
            title = "Updated Title",
            description = "Updated Desc",
            deadlineUtc = DateTime.UtcNow.AddDays(7),
            categoryId = catId,
            statusId = statusId
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify updated
        var getResp = await _client.GetAsync($"/api/tenders/{tid}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResp.Content.ReadFromJsonAsync<TenderDetailsDto>();
        dto!.Title.Should().Be("Updated Title");
        dto.Description.Should().Be("Updated Desc");
        dto.DeadlineUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Vendor_Cannot_Update_Tender()
    {
        // Register vendor
        var email = $"vendor+{Guid.NewGuid()}@test.com";
        var pwd = "Vendor123!";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Vendor"
        });
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var res = await login.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", res!.Token);

        // Try update (should be forbidden)
        var updateResp = await _client.PutAsJsonAsync($"/api/tenders/{Guid.NewGuid()}", new
        {
            id = Guid.NewGuid(),
            title = "Nope",
            description = "Nope",
            deadlineUtc = DateTime.UtcNow.AddDays(10),
            categoryId = Guid.NewGuid(),
            statusId = Guid.NewGuid()
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_With_Invalid_CategoryId_Should_Be_BadRequest()
    {
        var (token, catId, statusId) = await RegisterAndLoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create tender
        var createResp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Test",
            description = "Desc",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = catId,
            statusId = statusId
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var tid = (await createResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Update with invalid category
        var updateResp = await _client.PutAsJsonAsync($"/api/tenders/{tid}", new
        {
            id = tid,
            title = "X",
            description = "Y",
            deadlineUtc = DateTime.UtcNow.AddDays(5),
            categoryId = Guid.NewGuid(), // Invalid
            statusId = statusId
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_With_Invalid_StatusId_Should_Be_BadRequest()
    {
        var (token, catId, statusId) = await RegisterAndLoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create tender
        var createResp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Test",
            description = "Desc",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = catId,
            statusId = statusId
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var tid = (await createResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Update with invalid status
        var updateResp = await _client.PutAsJsonAsync($"/api/tenders/{tid}", new
        {
            id = tid,
            title = "X",
            description = "Y",
            deadlineUtc = DateTime.UtcNow.AddDays(5),
            categoryId = catId,
            statusId = Guid.NewGuid() // Invalid
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Nonexistent_Tender_Should_Be_NotFound()
    {
        var (token, catId, statusId) = await RegisterAndLoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateResp = await _client.PutAsJsonAsync($"/api/tenders/{Guid.NewGuid()}", new
        {
            id = Guid.NewGuid(),
            title = "Doesn't Exist",
            description = "Nope",
            deadlineUtc = DateTime.UtcNow.AddDays(5),
            categoryId = catId,
            statusId = statusId
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_With_Invalid_Fields_Should_Be_BadRequest()
    {
        var (token, catId, statusId) = await RegisterAndLoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create tender
        var createResp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Test",
            description = "Desc",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = catId,
            statusId = statusId
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var tid = (await createResp.Content.ReadFromJsonAsync<CreateResult>())!.Id;

        // Missing required field: title
        var updateResp = await _client.PutAsJsonAsync($"/api/tenders/{tid}", new
        {
            id = tid,
            // title missing
            description = "No Title",
            deadlineUtc = DateTime.UtcNow.AddDays(2),
            categoryId = catId,
            statusId = statusId
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // DTOs
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
        public DateTime SubmittedAtUtc { get; set; }
        public VendorDto Vendor { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
    }
    public class VendorDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
}

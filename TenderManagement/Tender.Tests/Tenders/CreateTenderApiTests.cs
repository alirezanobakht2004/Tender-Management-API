using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Tenders;

public sealed class CreateTenderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CreateTenderApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    private async Task<string> RegisterAndLoginAsAdmin()
    {
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "ApiTest123!";

        var regResp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Admin"
        });
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResult>();
        return login!.Token;
    }

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

    [Fact]
    public async Task Admin_Can_Create_Tender()
    {
        var token = await RegisterAndLoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var catId = await GetCategoryId();
        var statusId = await GetStatusId();

        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Tender title",
            description = "Tender description",
            deadlineUtc = DateTime.UtcNow.AddDays(5),
            categoryId = catId,
            statusId = statusId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await resp.Content.ReadFromJsonAsync<CreateResult>();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Non_Admin_Is_Forbidden()
    {
        // Register/login as Vendor
        var email = $"vendor+{Guid.NewGuid()}@test.com";
        var pwd = "VendorTest123!";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = pwd,
            role = "Vendor"
        });
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = pwd
        });
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.Token);

        var catId = await GetCategoryId();
        var statusId = await GetStatusId();

        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = "Tender title",
            description = "Tender description",
            deadlineUtc = DateTime.UtcNow.AddDays(5),
            categoryId = catId,
            statusId = statusId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Missing_Required_Fields_Returns_400()
    {
        var token = await RegisterAndLoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            // title missing
            description = "desc",
            deadlineUtc = DateTime.UtcNow.AddDays(5),
            categoryId = Guid.NewGuid(),
            statusId = Guid.NewGuid()
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public class CreateResult { public Guid Id { get; set; } }
    public class LoginResult { public string Token { get; set; } = ""; }
    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
}
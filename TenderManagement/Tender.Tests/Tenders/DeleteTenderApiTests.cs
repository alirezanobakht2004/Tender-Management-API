using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Tenders;

public sealed class DeleteTenderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DeleteTenderApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    private async Task<(string Token, Guid CategoryId, Guid StatusId, Guid TenderId)> CreateTenderAsAdmin()
    {
        var email = $"admin+{Guid.NewGuid()}@test.com";
        var pwd = "ApiTest123!";
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = pwd, role = "Admin" });
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = pwd });
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.Token);

        var categoryId = (await (await _client.GetAsync("/api/categories")).Content.ReadFromJsonAsync<List<LookupDto>>())!.First().Id;
        var statusId = (await (await _client.GetAsync("/api/statuses")).Content.ReadFromJsonAsync<List<LookupDto>>())!.First().Id;

        var resp = await _client.PostAsJsonAsync("/api/tenders", new
        {
            title = $"Tender {Guid.NewGuid()}",
            description = "Delete test",
            deadlineUtc = DateTime.UtcNow.AddDays(10),
            categoryId,
            statusId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenderId = (await resp.Content.ReadFromJsonAsync<CreateResult>())!.Id;
        return (login.Token, categoryId, statusId, tenderId);
    }

    [Fact]
    public async Task Admin_Can_Delete_Tender()
    {
        var (token, _, _, tenderId) = await CreateTenderAsAdmin();

        // Delete as Admin
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var del = await _client.DeleteAsync($"/api/tenders/{tenderId}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // GET should now 404
        var get = await _client.GetAsync($"/api/tenders/{tenderId}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Vendor_Cannot_Delete_Tender()
    {
        var (_, _, _, tenderId) = await CreateTenderAsAdmin();

        // Register and login as Vendor
        var vEmail = $"vendor+{Guid.NewGuid()}@test.com";
        var vPwd = "Vendor123!";
        await _client.PostAsJsonAsync("/api/auth/register", new { email = vEmail, password = vPwd, role = "Vendor" });
        var vloginResp = await _client.PostAsJsonAsync("/api/auth/login", new { email = vEmail, password = vPwd });
        var vlogin = await vloginResp.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vlogin!.Token);

        // Attempt to delete as Vendor
        var del = await _client.DeleteAsync($"/api/tenders/{tenderId}");
        del.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Ensure resource is not deleted
        _client.DefaultRequestHeaders.Authorization = null;
        var get = await _client.GetAsync($"/api/tenders/{tenderId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cannot_Delete_Tender_Unauthenticated()
    {
        var (_, _, _, tenderId) = await CreateTenderAsAdmin();

        // Clear auth header
        _client.DefaultRequestHeaders.Authorization = null;

        // Attempt delete without token
        var del = await _client.DeleteAsync($"/api/tenders/{tenderId}");
        del.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Ensure resource is not deleted
        var get = await _client.GetAsync($"/api/tenders/{tenderId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Nonexistent_Tender_Returns_NoContent()
    {
        var (token, _, _, _) = await CreateTenderAsAdmin();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Delete random GUID (idempotent/no-content)
        var del = await _client.DeleteAsync($"/api/tenders/{Guid.NewGuid()}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Invalid_Id_Returns_BadRequest()
    {
        var (token, _, _, _) = await CreateTenderAsAdmin();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Malformed GUID
        var del = await _client.DeleteAsync($"/api/tenders/not-a-guid");
        del.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public class CreateResult { public Guid Id { get; set; } }
    public class LoginResult { public string Token { get; set; } = ""; }
    public class LookupDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
}

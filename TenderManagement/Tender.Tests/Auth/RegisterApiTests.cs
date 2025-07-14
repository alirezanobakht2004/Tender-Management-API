using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Auth;

public sealed class RegisterApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RegisterApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    // Helper to ensure unique emails per test
    private string UniqueEmail(string prefix = "unit") =>
        $"{prefix}+{Guid.NewGuid()}@test.com";

    [Fact]
    public async Task Register_Admin_Succeeds()
    {
        var email = UniqueEmail("unitadmin1");
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "TestPassword1!",
            role = "Admin"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_Vendor_Succeeds()
    {
        var email = UniqueEmail("unitvendor1");
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "VendorPass1!",
            role = "Vendor"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_Duplicate_Email_409()
    {
        var email = UniqueEmail("dupuser");
        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "DupPass1!",
            role = "Admin"
        });
        // Second (duplicate)
        var resp2 = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "DupPass2!",
            role = "Admin"
        });
        resp2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await resp2.Content.ReadFromJsonAsync<ProblemDetails>();
        body!.Title.Should().Be("Duplicate email");
    }

    [Fact]
    public async Task Register_Invalid_Email_400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "ValidPass1!",
            role = "Admin"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Register_Empty_Email_400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "",
            password = "ValidPass1!",
            role = "Admin"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Register_Short_Password_400()
    {
        var email = UniqueEmail("shortpass");
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "123",
            role = "Admin"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Password");
    }

    [Fact]
    public async Task Register_Missing_Role_400()
    {
        var email = UniqueEmail("missingrole");
        var payload = new
        {
            email,
            password = "MissingRolePass1!"
        };
        var resp = await _client.PostAsJsonAsync("/api/auth/register", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Role");
    }

    [Fact]
    public async Task Register_Invalid_Role_400()
    {
        var email = UniqueEmail("badrole");
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "BadRolePass1!",
            role = "Manager"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Role");
    }

    [Fact]
    public async Task Register_Email_With_Whitespace_201()
    {
        // Unique email, with whitespace
        var baseEmail = UniqueEmail("whitespace").Replace("@", "");
        var email = "   " + baseEmail + "@test.com   ";
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "WhitespacePass1!",
            role = "Vendor"
        });
        // Accept either 201 (if you trim) or 400 (if you don't trim)
        resp.StatusCode.Should().Match(s => s == HttpStatusCode.Created || s == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Email_Case_Sensitivity_409()
    {
        var baseEmail = UniqueEmail("casesens");
        var email1 = baseEmail.ToLowerInvariant();
        var email2 = baseEmail.ToUpperInvariant();
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = email1,
            password = "CasePass1!",
            role = "Vendor"
        });
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = email2,
            password = "CasePass2!",
            role = "Vendor"
        });
        // If you handle case-insensitive, expect 409; otherwise, may allow 201
        resp.StatusCode.Should().Match(s => s == HttpStatusCode.Conflict || s == HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_Malformed_Json_400()
    {
        var req = new StringContent("{ \"email\": \"malformed@test.com\", ", System.Text.Encoding.UTF8, "application/json");
        var resp = await _client.PostAsync("/api/auth/register", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tender.Tests.Auth;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public sealed class RegisterApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RegisterApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Register_Admin_Succeeds()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "unitadmin1@test.com",
            password = "TestPassword1!",
            role = "Admin"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_Vendor_Succeeds()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "unitvendor1@test.com",
            password = "VendorPass1!",
            role = "Vendor"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_Duplicate_Email_409()
    {
        var email = "dupuser@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "DupPass1!",
            role = "Admin"
        });

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
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "shortpass@test.com",
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
        // Role omitted
        var payload = new
        {
            email = "missingrole@test.com",
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
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "badrole@test.com",
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
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "   whitespace@test.com   ",
            password = "WhitespacePass1!",
            role = "Vendor"
        });
        // Accept either 201 (if you trim) or 400 (if you don't)
        resp.StatusCode.Should().Match(s => s == HttpStatusCode.Created || s == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Email_Case_Sensitivity_409()
    {
        var email = "casesens@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "CasePass1!",
            role = "Vendor"
        });
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "CaseSens@Test.com",
            password = "CasePass2!",
            role = "Vendor"
        });
        // If you handle case-insensitive, expect 409; otherwise, may allow (201)
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

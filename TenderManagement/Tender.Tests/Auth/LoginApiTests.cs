using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tender.Tests.Auth;

// Helper class to deserialize login result
public class LoginResult { public string Token { get; set; } = ""; }

public sealed class LoginApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LoginApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    // Generates a unique email per test run
    private string UniqueEmail(string basePrefix = "apitest") =>
        $"{basePrefix}+{Guid.NewGuid()}@login.com";

    private async Task RegisterUser(string email, string password, string role)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password,
            role
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login_Succeeds()
    {
        string email = UniqueEmail("loginuser");
        string password = "ApiTest123!";
        await RegisterUser(email, password, "Vendor");

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await resp.Content.ReadFromJsonAsync<LoginResult>();
        result!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Wrong_Password_401()
    {
        string email = UniqueEmail("wrongpass");
        await RegisterUser(email, "GoodPass123!", "Vendor");

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "WrongPassword!"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        body!.Title.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_Unknown_Email_401()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = UniqueEmail("nouser"),
            password = "Whatever123!"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        body!.Title.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_Empty_Email_400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "",
            password = "Anything123!"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Login_Empty_Password_400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = UniqueEmail("emptypass"),
            password = ""
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body!.Errors.Should().ContainKey("Password");
    }

    [Fact]
    public async Task Login_Malformed_Json_400()
    {
        var req = new StringContent("{ \"email\": \"malformed@login.com\", ", System.Text.Encoding.UTF8, "application/json");
        var resp = await _client.PostAsync("/api/auth/login", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
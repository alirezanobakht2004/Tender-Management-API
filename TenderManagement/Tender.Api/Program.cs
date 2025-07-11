using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Contracts;
using Tender.Infrastructure.Persistence;
using Tender.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Tender.Application;
using Tender.Application.Pipeline;
using Microsoft.Data.SqlClient;
using System.Data;
using Tender.Infrastructure.ReadModels;
using Tender.Application.Queries.Tenders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tender.Application.Auth;
using Tender.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Tender.Domain.Entities;
using Tender.Application.Queries.Tenders.List;
using Tender.Application.Queries.Lookups;
using Tender.Application.Queries.Vendors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TenderDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("TenderDb")));

// Program.cs  (add these lines after AddDbContext, before Build)

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenderRepository, TenderRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();

// Tender.Api/Program.cs  (relevant section only)

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>());

builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>),
                              typeof(ValidationBehaviour<,>));

builder.Services.AddTransient<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("TenderDb")));
builder.Services.AddTransient<IGetTenderWithBidsQuery, GetTenderWithBidsQueryObject>();

// Configure the HTTP request pipeline.

var jwtCfgSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtCfgSection);
var jwtCfg = jwtCfgSection.Get<JwtSettings>()!;      
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtCfg.Issuer,
            ValidAudience = jwtCfg.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtCfg.Secret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Auth helpers
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddTransient<ITenderListQuery, TenderListQuery>();

builder.Services.AddTransient<ICategoryListQuery, CategoryListQuery>();
builder.Services.AddTransient<IStatusListQuery, StatusListQuery>();

builder.Services.AddTransient<IVendorListQuery, VendorListQuery>();

builder.Services.AddTransient<IVendorDetailsQuery, VendorDetailsQuery>();


var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

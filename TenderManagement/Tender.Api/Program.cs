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

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

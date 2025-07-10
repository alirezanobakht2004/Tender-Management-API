using Microsoft.EntityFrameworkCore;
using Tender.Domain.Contracts.Repositories;
using Tender.Domain.Contracts;
using Tender.Infrastructure.Persistence;
using Tender.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Tender.Application;
using Tender.Application.Pipeline;

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


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

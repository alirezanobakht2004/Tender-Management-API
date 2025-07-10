using Microsoft.EntityFrameworkCore;
using Tender.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<TenderDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("TenderDb")));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

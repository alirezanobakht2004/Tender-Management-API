using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tender.Domain.Entities;

using TenderEntity = Tender.Domain.Entities.Tender;

namespace Tender.Infrastructure.Persistence;

public class TenderDbContext : DbContext
{
    public TenderDbContext(DbContextOptions<TenderDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<TenderEntity> Tenders => Set<TenderEntity>();
    public DbSet<Bid> Bids => Set<Bid>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // RowVersion
        foreach (var entity in builder.Model.GetEntityTypes())
            if (entity.FindProperty("RowVersion") is { } row) row.IsConcurrencyToken = true;

        // Unique Bid per Vendor per Tender
        builder.Entity<Bid>()
               .HasIndex(b => new { b.TenderId, b.VendorId })
               .IsUnique();

        // Decimal precision for Money
        builder.Entity<Bid>()
               .OwnsOne(b => b.BidAmount, m => m.Property(p => p.Value)
                                                 .HasColumnType("decimal(18,2)")
                                                 .HasColumnName("BidAmount"));

        // Value-object conversion for Deadline
        builder.Entity<TenderEntity>()
               .OwnsOne(t => t.Deadline, d => d.Property(p => p.Value)
                                               .HasColumnName("Deadline"));

        // TODO: seed Category and Status lookup tables here in a later step
    }
}

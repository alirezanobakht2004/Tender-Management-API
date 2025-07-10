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
        // RowVersion – already in your code
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
            {
                builder.Entity(entity.ClrType)
                       .Property<byte[]>("RowVersion")
                       .IsRowVersion()               // maps to SQL rowversion / timestamp
                       .IsConcurrencyToken()
                       .ValueGeneratedOnAddOrUpdate();
            }
        }

        // === Default timestamps (Option 1) ===
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
            {
                builder.Entity(entity.ClrType)
                       .Property<DateTime>("CreatedAt")
                       .HasDefaultValueSql("GETUTCDATE()");

                builder.Entity(entity.ClrType)
                       .Property<DateTime>("UpdatedAt")
                       .HasDefaultValueSql("GETUTCDATE()");
            }
        }

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

        // example constant chosen once for all seeds
        var stamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Category seed
        builder.Entity<Category>().HasData(
            new
            {
                Id = Guid.Parse("e2ae4a2e-5a74-4f2a-b8ba-1b0bfc859101"),
                Name = "Construction",
                Description = "Civil and structural works",
                CreatedAt = stamp,
                UpdatedAt = stamp
            },
            new
            {
                Id = Guid.Parse("c6c2ae11-0b1a-4220-99eb-bc69f2c46d77"),
                Name = "IT",
                Description = "Software and hardware",
                CreatedAt = stamp,
                UpdatedAt = stamp
            },
            new
            {
                Id = Guid.Parse("54d06027-6965-456b-8b6e-1bc5e1e3c2af"),
                Name = "Consulting",
                Description = "Professional services",
                CreatedAt = stamp,
                UpdatedAt = stamp
            });

        builder.Entity<Status>().HasData(
            new
            {
                Id = Guid.Parse("a1a3f451-a73b-484e-8fcb-0c21821f4d48"),
                Name = "Open",
                Scope = "Tender",
                SortOrder = 1,
                CreatedAt = stamp,
                UpdatedAt = stamp
            },
            new
            {
                Id = Guid.Parse("8b6cc0d4-c1ce-4991-a5fb-84f7ef404ac1"),
                Name = "Closed",
                Scope = "Tender",
                SortOrder = 2,
                CreatedAt = stamp,
                UpdatedAt = stamp
            },
            new
            {
                Id = Guid.Parse("41d9b6d9-fd37-4894-a63e-65892a0cfe19"),
                Name = "Pending",
                Scope = "Bid",
                SortOrder = 1,
                CreatedAt = stamp,
                UpdatedAt = stamp
            },
            new
            {
                Id = Guid.Parse("d7122c1f-e7e8-4476-a2e2-19a2d906f6af"),
                Name = "Approved",
                Scope = "Bid",
                SortOrder = 2,
                CreatedAt = stamp,
                UpdatedAt = stamp
            },
            new
            {
                Id = Guid.Parse("e022521e-ec06-4cf2-83f8-61eb0cd8a0f2"),
                Name = "Rejected",
                Scope = "Bid",
                SortOrder = 3,
                CreatedAt = stamp,
                UpdatedAt = stamp
            });

    }
}

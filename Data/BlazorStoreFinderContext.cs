#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BlazorStoreFinder
{
    public partial class BlazorStoreFinderContext : DbContext
    {
        public BlazorStoreFinderContext()
        {
        }

        public BlazorStoreFinderContext(
            DbContextOptions<BlazorStoreFinderContext> options)
            : base(options)
        {
        }

        public virtual DbSet<StoreLocations> StoreLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreLocations>(entity =>
            {
                entity.Property(e => e.LocationAddress)
                .IsRequired()
                .HasMaxLength(250);

                entity.Property(e => e.LocationLatitude)
                .IsRequired()
                .HasMaxLength(50);

                entity.Property(e => e.LocationLongitude)
                .IsRequired()
                .HasMaxLength(50);

                entity.Property(e => e.LocationName)
                .IsRequired()
                .HasMaxLength(50);

                entity.Property(e => e.LocationData)
                .IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
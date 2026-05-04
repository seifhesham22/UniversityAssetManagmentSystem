using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Models;

namespace UAMS.Room.Presistence
{
    public class RoomDesignDbContext : DbContext
    {
        public DbSet<AssetChecklist> CheckLists => Set<AssetChecklist>();
        public DbSet<AssetDefenetion> AssetDefinitions => Set<AssetDefenetion>();
        public DbSet<ChecklistEntry> ChecklistEntries => Set<ChecklistEntry>();
        public DbSet<ChecklistItemTemplate> CheckListItemTemplates => Set<ChecklistItemTemplate>();
        public DbSet<PlacedAssetEntry> PlacedAssetEntries => Set<PlacedAssetEntry>();
        public DbSet<UAMS.Room.Models.Room> Rooms => Set<UAMS.Room.Models.Room>();
        public DbSet<RoomLayout> RoomLayouts => Set<RoomLayout>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketNote> TicketNotes => Set<TicketNote>();

        public RoomDesignDbContext(DbContextOptions<RoomDesignDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("roomdesign_ticketing");

            modelBuilder.Entity<UAMS.Room.Models.Room>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.Name).HasMaxLength(200);
                e.Property(r => r.ClosureReason).HasMaxLength(500);
                e.HasIndex(r => r.FacultyId);

                e.OwnsOne(r => r.Layout, layout =>
                {
                    layout.ToJson("Layout");
                    layout.OwnsMany(l => l.PlacedAssets);
                });
            });
            base.OnModelCreating(modelBuilder);
        }


    }
}

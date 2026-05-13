using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Models;
using UAMS.Room.Models.Enums;

namespace UAMS.Room.Presistence
{
    public class RoomDesignDbContext : DbContext
    {
        public DbSet<UAMS.Room.Models.Room> Rooms => Set<UAMS.Room.Models.Room>();
        public DbSet<AssetDefenetion> AssetDefinitions => Set<AssetDefenetion>();
        public DbSet<ChecklistItemTemplate> CheckListItemTemplates => Set<ChecklistItemTemplate>();
        public DbSet<PlacedAssetChecklist> PlacedAssetCheckLists => Set<PlacedAssetChecklist>();
        public DbSet<PlacedAssetChecklistEntry> PlacedAssetCheckListEntries => Set<PlacedAssetChecklistEntry>();
        //public DbSet<PlacedAssetEntry> PlacedAssetEntries => Set<PlacedAssetEntry>();
        
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketNote> TicketNotes => Set<TicketNote>();

        public RoomDesignDbContext(DbContextOptions<RoomDesignDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("RoomDesign");
            modelBuilder.Entity<UAMS.Room.Models.Room>(e =>
            {
                e.ToTable("Rooms");

                e.HasKey(r => r.Id);

                e.Property(r => r.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(r => r.ClosureReason)
                    .HasMaxLength(500);

                e.HasIndex(r => r.FacultyId);

                e.OwnsOne(r => r.Layout, layout =>
                {
                    layout.ToJson("Layout");

                    layout.Property(l => l.Version);

                    layout.Property(l => l.LastModifiedUserId);

                    layout.Property(l => l.LastModifiedDate);

                    layout.OwnsMany(l => l.PlacedAssets, pa =>
                    {
                        pa.Property(a => a.Id);

                        //pa.HasKey(a => a.Id);

                        pa.Property(a => a.AssetName)
                            .HasMaxLength(200)
                            .IsRequired();

                        pa.Property(a => a.X);

                        pa.Property(a => a.Y);

                        pa.Property(a => a.Width);

                        pa.Property(a => a.Height);

                        pa.Property(a => a.Condition)
                            .HasConversion<int>();
                    });
                });
            });


            modelBuilder.Entity<AssetDefenetion>(e =>
            {
                e.ToTable("AssetDefinitions");

                e.HasKey(a => a.Id);

                e.Property(a => a.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(a => a.SvgUrl)
                    .HasMaxLength(1000)
                    .IsRequired();

                e.Property(a => a.Category)
                    .HasConversion<int>();

                
                e.Property(x => x.AllowedLocations).HasColumnName("AssetAllowedLocations");


                e.Navigation(a => a.ChecklistTemplate)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                e.HasMany(a => a.ChecklistTemplate)
                    .WithOne(ci => ci.AssetDefenetion)
                    .HasForeignKey(ci => ci.AssetDefinitionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });



            modelBuilder.Entity<ChecklistItemTemplate>(e =>
            {
                e.ToTable("ChecklistItemTemplates");

                e.HasKey(ci => ci.Id);

                e.Property(ci => ci.Description)
                    .HasMaxLength(500)
                    .IsRequired();
            });


            modelBuilder.Entity<PlacedAssetChecklist>(e =>
            {
                e.ToTable("PlacedAssetChecklists");

                e.HasKey(ac => ac.Id);

                e.Property(ac => ac.StudyYear)
                    .HasMaxLength(20)
                    .IsRequired();

                e.Navigation(a => a.Entries)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                e.HasMany(ac => ac.Entries)
                    .WithOne(ce => ce.AssetChecklist)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<PlacedAssetChecklistEntry>(e =>
            {
                e.ToTable("PlacedAssetChecklistEntries");

                e.HasKey(ce => ce.Id);

                e.HasOne(ce => ce.ChecklistItem)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Ticket>(e =>
            {
                e.ToTable("Tickets");

                e.HasKey(t => t.Id);

                e.HasIndex(t => t.FacultyId);

                e.HasIndex(t => t.ReporterId);

                e.HasIndex(t => t.DepartmentId);

                e.Property(t => t.Status)
                    .HasConversion<int>();

                e.Property(t => t.Decision)
                    .HasConversion<int>();

                e.Navigation(t => t.TicketNotes)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                e.HasMany(t => t.TicketNotes)
                    .WithOne()
                    .HasForeignKey(n => n.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(t => t.PlacedAssetId)
                    .IsUnique()
                    .HasFilter(
                        $"\"Status\" NOT IN (" +
                        $"{(int)TicketStatus.ConfirmedFixed}, " +
                        $"{(int)TicketStatus.EscalatedExternally}, " +
                        $"{(int)TicketStatus.Irreparable}, " +
                        $"{(int)TicketStatus.Closed})");
            });

            modelBuilder.Entity<TicketNote>(e =>
            {
                e.ToTable("TicketNotes");

                e.HasKey(n => n.Id);

                e.Property(n => n.Content)
                    .HasMaxLength(2000)
                    .IsRequired();
            });
        }
    }
}
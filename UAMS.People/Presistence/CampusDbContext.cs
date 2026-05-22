using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;

namespace UAMS.Campus.Presistence
{
    public class CampusDbContext : DbContext
    {
        public DbSet<AssetManager> asset_managers => Set<AssetManager>();
        public DbSet<Building> buildings => Set<Building>();
        public DbSet<DepartmentManager> department_managers => Set<DepartmentManager>();
        public DbSet<Maintainer> maintainers => Set<Maintainer>();
        public DbSet<Student> students => Set<Student>();
        public DbSet<Teacher> teachers => Set<Teacher>();
        public DbSet<TeacherFaculty> teacherFaculties => Set<TeacherFaculty>();
        public DbSet<Department> departments => Set<Department>();
        public DbSet<Faculty> faculties => Set<Faculty>();
        public DbSet<FacultyBuilding> facultyBuildings => Set<FacultyBuilding>();

        public CampusDbContext(DbContextOptions<CampusDbContext> options) : base(options) { }

        protected CampusDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("campus");

            modelBuilder.Entity<Building>(e =>
            {
                e.HasKey(b => b.Id);
                e.Property(b => b.Name).HasMaxLength(200);
                e.Property(b => b.Address).HasMaxLength(500);
                e.HasMany(b => b.FacultyLinks).WithOne(fb => fb.Building)
                    .HasForeignKey(fb => fb.BuildingId);
            });

            modelBuilder.Entity<Faculty>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.Name).HasMaxLength(200);
                e.HasMany(f => f.BuildingLinks).WithOne(fb => fb.Faculty)
                    .HasForeignKey(fb => fb.FacultyId);
                e.HasMany(f => f.Students).WithOne(s => s.Faculty)
                    .HasForeignKey(s => s.FacultyId);
                e.HasMany(f => f.AssetManagers).WithOne(a => a.Faculty)
                    .HasForeignKey(a => a.FacultyId);
            });

            modelBuilder.Entity<FacultyBuilding>(e =>
            e.HasKey(fb => new { fb.FacultyId, fb.BuildingId }));

            modelBuilder.Entity<Department>(e =>
            {
                e.HasKey(d => d.Id);
                e.Property(d => d.Name).HasMaxLength(200);
                e.HasIndex(d => d.DepartmentManagerId).IsUnique();
                e.HasOne(d => d.Manager).WithOne(dm => dm.Department)
                    .HasForeignKey<Department>(d => d.DepartmentManagerId);
                e.HasMany(d => d.Maintainers).WithOne(m => m.Department)
                    .HasForeignKey(m => m.DepartmentId);
            });

            modelBuilder.Entity<Student>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasIndex(s => s.UserId).IsUnique();
                e.Property(s => s.FullName).HasMaxLength(200);
            });


            modelBuilder.Entity<Teacher>(e =>
            {
                e.HasKey(t => t.Id);
                e.HasIndex(t => t.UserId).IsUnique();
                e.Property(t => t.FullName).HasMaxLength(200);
                e.HasMany(t => t.Faculties).WithOne(tf => tf.Teacher)
                    .HasForeignKey(tf => tf.TeacherId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TeacherFaculty>(e =>
                e.HasKey(tf => new { tf.TeacherId, tf.FacultyId }));

            modelBuilder.Entity<AssetManager>(e =>
            {
                e.HasKey(a => a.Id);
                e.HasIndex(a => a.UserId).IsUnique();
                e.Property(a => a.FullName).HasMaxLength(200);
            });

            modelBuilder.Entity<DepartmentManager>(e =>
            {
                e.HasKey(d => d.Id);
                e.HasIndex(d => d.UserId).IsUnique();
                e.Property(d => d.FullName).HasMaxLength(200);
            });

            modelBuilder.Entity<Maintainer>(e =>
            {
                e.HasKey(m => m.Id);
                e.HasIndex(m => m.UserId).IsUnique();
                e.Property(m => m.FullName).HasMaxLength(200);
                e.Property(m => m.VkId).HasMaxLength(50);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
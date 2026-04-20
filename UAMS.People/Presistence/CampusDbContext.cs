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
        public DbSet<AssetManager> asset_managers {  get; set; }
        public DbSet<Building> buildings { get; set; }
        public DbSet<DepartmentManager> department_managers { get; set; }
        public DbSet<Maintainer> maintainers { get; set; }
        public DbSet<Student> students { get; set; }
        public DbSet<Teacher> teachers {  get; set; }
        public DbSet<TeacherFaculty> teacherFaculties { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<Faculty> faculties { get; set; }
        public CampusDbContext(DbContextOptions options) : base(options) { }

        protected CampusDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
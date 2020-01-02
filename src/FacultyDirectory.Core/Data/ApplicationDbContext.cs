using System;
using System.Collections.Generic;
using System.Text;
using FacultyDirectory.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<SitePerson> SitePeople { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<PersonSource> PeopleSources { get; set; }
        public DbSet<SiteTag> SiteTags { get; set; }
        public DbSet<SitePersonTag> SitePeopleTags { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Person>().HasIndex(p => p.IamId).IsUnique();
        }
    }
}

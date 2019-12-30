using System;
using System.Collections.Generic;
using System.Text;

namespace FacultyDirectory.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}

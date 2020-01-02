using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class Site {
        public int Id { get; set; }

        [StringLength (64)]
        public string Name { get; set; }

        [StringLength (64)]
        public string Url { get; set; }

        public string Creds { get; set; }
    }
}
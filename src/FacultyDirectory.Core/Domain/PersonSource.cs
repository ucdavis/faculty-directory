using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class PersonSource {
        public int Id { get; set; }

        public int PersonId { get; set; }

        [Required]
        public Person Person { get; set; }

        // google, orcid, etc
        [Required]
        [StringLength (64)]
        public string Source { get; set; }

        [Required]
        [StringLength(128)]
        public string SourceKey { get; set; }

        // JSON
        public string Data { get; set; }

        public DateTime? LastUpdate { get; set; }

        public bool HasKeywords { get; set; }

        public bool HasPubs { get; set; }

        public bool HasBio { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class SitePerson {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public Person Person { get; set; }

        public int SiteId { get; set; }

        public Site Site { get; set; }

        public Guid? PageUid { get; set; }

        public string Bio { get; set; }

        [StringLength (256)]
        public string Name { get; set; }

        public bool ShouldSync { get; set; }

        public DateTime? LastSync { get; set; }

        public DateTime? LastUpdate { get; set; }
    }
}
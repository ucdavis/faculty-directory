using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class SiteTag {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public Site Site { get; set; }

        public Guid TagUid { get; set; }

        [StringLength (64)]
        public string Name { get; set; }
    }
}
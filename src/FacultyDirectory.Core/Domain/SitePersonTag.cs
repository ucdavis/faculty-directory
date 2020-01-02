using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class SitePersonTag {
        public int SitePersonId { get; set; }

        public SitePerson SitePerson { get; set; }

        public int SiteTagId { get; set; }

        public SiteTag SiteTag { get; set; }

        [StringLength (64)]
        public string Name { get; set; }

        [StringLength (64)]
        public string Source { get; set; }

        public bool Sync { get; set; }
    }
}
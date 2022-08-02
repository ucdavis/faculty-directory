using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain
{
    public class SitePerson
    {
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }

        public Person Person { get; set; }

        [Required]
        public int SiteId { get; set; }

        public Site Site { get; set; }

        public Guid? PageUid { get; set; }

        public string Bio { get; set; }

        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(128)]
        public string LastName { get; set; }

        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Emails { get; set; }

        [StringLength(512)]
        public string Phones { get; set; }

        public string Websites { get; set; }

        [StringLength(64)]
        public Guid? PronunciationUid { get; set; }

        [StringLength(256)]
        public string Title { get; set; }

        public string Departments { get; set; }

        public string Tags { get; set; }

        public bool ShouldSync { get; set; }

        public DateTime? LastSync { get; set; }

        public DateTime? LastUpdate { get; set; }
    }
}
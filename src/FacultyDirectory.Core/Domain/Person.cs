using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FacultyDirectory.Core.Domain {
    public class Person {
        public int Id { get; set; }

        [Required]
        [StringLength(16)]
        public string IamId { get; set; }

        [StringLength(32)]
        public string Kerberos { get; set; }

        [StringLength(128)]

        public string FirstName { get; set; }

        [StringLength(128)]
        public string LastName { get; set; }

        [StringLength(256)]
        public string FullName { get; set; }

        [StringLength(128)]
        public string Email { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [StringLength(256)]
        public string Title { get; set; }

        [StringLength(256)]
        public string Departments { get; set; }

        // Faculty or Emeriti
        [StringLength(16)]
        public string Classification { get; set; }

        public List<PersonSource> Sources { get; set; }

        [JsonIgnore]
        // all site specific instances of this person
        public List<SitePerson> SitePeople { get; set; }
    }
}
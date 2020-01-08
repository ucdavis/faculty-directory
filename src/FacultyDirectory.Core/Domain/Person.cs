using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class Person {
        public int Id { get; set; }

        // [Required]
        public string IamId { get; set; }

        public string Kerberos { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public string Departments { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain {
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(256)]
        public string Username { get; set; }
    }
}

namespace FacultyDirectory.Core.Domain {
    public class Person {
        public int Id { get; set; }

        // TODO: unique
        public string IamId { get; set; }

        public string Kerberos { get; set; }
        
    }
}
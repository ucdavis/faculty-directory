using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyDirectory.Core.Domain
{
    public class DrupalPerson
    {
        public Guid? PageUid { get; set; }

        public string Bio { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Title { get; set; }

        public string[] Emails { get; set; }
        public string[] Phones { get; set; }
        public string[] Departments { get; set; }
        public string[] Tags { get; set; }
        public DrupalWebsite[] Websites { get; set; }
    }

    public class DrupalWebsite {
        public string Uri { get; set; }
        public string Title { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace ModernApiProject.Domain.Entities
{
    public class Student
    {
        [Key]
        public string StudentRegno { get; set; }
        public string StudentPhoto { get; set; }
        public string Password { get; set; }
        public string StudentName { get; set; }
        public string Pincode { get; set; }
        public string Session { get; set; }
        public string Department { get; set; }
        public string Semester { get; set; }
        public decimal Cgpa { get; set; }
        public DateTime Creationdate { get; set; }
        public string? UpdationDate { get; set; }
    }
}

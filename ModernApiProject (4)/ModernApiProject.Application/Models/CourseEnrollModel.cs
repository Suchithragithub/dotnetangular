using System;

namespace ModernApiProject.Application.Models
{
    public class CourseenrollModel
    {
        public int Id { get; set; }
        public string StudentRegno { get; set; }
        public string Pincode { get; set; }
        public int Session { get; set; }
        public int Department { get; set; }
        public int Level { get; set; }
        public int Semester { get; set; }
        public int Course { get; set; }
        public DateTime EnrollDate { get; set; }
    }
}

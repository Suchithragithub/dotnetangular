using System;
using System.ComponentModel.DataAnnotations;

namespace ModernApiProject.Domain.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseUnit { get; set; }
        public int NoofSeats { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdationDate { get; set; }
    }
}

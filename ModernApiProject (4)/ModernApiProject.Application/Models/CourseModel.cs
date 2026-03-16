using System;

namespace ModernApiProject.Application.Models
{
    public class CourseModel
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseUnit { get; set; }
        public int NoofSeats { get; set; }
        public DateTime CreationDate { get; set; }
        public string? UpdationDate { get; set; }
    }
}

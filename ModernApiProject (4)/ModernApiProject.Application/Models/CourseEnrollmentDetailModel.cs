// using System;

// namespace ModernApiProject.Application.Models
// {
//     public class CourseEnrollmentDetailModel
//     {
//         public object? CourseName { get; set; }
//         public object? CourseCode { get; set; }
//         public object? CourseUnit { get; set; }
//         public object? SessionName { get; set; }
//         public object? DepartmentName { get; set; }
//         public object? LevelName { get; set; }
//         public object? EnrollDate { get; set; }
//         public object? SemesterName { get; set; }
//         public object? StudentName { get; set; }
//         public object? StudentPhoto { get; set; }
//         public object? Cgpa { get; set; }
//         public object? StudentRegistrationDate { get; set; }
//     }
// }


using System;

namespace ModernApiProject.Application.Models
{
    public class CourseEnrollmentDetailModel
    {
        public string? StudentRegno { get; set; }    // add
        public string? StudentName { get; set; }
        public int CourseId { get; set; }            // add
        public string? CourseName { get; set; }      // fix: object? → string?
        public string? CourseCode { get; set; }      // fix: object? → string?
        public string? CourseUnit { get; set; }      // fix: object? → string?
        public string? SessionName { get; set; }     // fix: object? → string?
        public string? DepartmentName { get; set; }  // fix: object? → string?
        public string? LevelName { get; set; }       // fix: object? → string?
        public string? SemesterName { get; set; }    // fix: object? → string?
        public DateTime EnrollDate { get; set; }     // fix: object? → DateTime
        public string? StudentPhoto { get; set; }    // fix: object? → string?
        public decimal? Cgpa { get; set; }           // fix: object? → decimal?
        public DateTime? StudentRegistrationDate { get; set; } // fix: object? → DateTime?
    }
}
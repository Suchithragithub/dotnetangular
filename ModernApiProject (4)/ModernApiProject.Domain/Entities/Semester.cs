using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModernApiProject.Domain.Entities
{
    public class Semester
    {
        [Key]
        public int Id { get; set; }
        [Column("semester")]
        public string SemesterName { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdationDate { get; set; }
    }
}

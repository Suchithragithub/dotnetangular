using System;
using System.ComponentModel.DataAnnotations;

namespace ModernApiProject.Application.Models
{
    public class SemesterModel
    {
        public int Id { get; set; }

        [Required]
        public string SemesterName { get; set; }

        public DateTime CreationDate { get; set; }
        public string? UpdationDate { get; set; }
    }
}

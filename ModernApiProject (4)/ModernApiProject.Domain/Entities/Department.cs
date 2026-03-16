using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModernApiProject.Domain.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        [Column("department")]
        public string DepartmentName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModernApiProject.Domain.Entities
{
    public class Level
    {
        [Key]
        public int Id { get; set; }
        [Column("level")]
        public string LevelName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

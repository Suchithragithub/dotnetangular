using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModernApiProject.Domain.Entities
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        [Column("session")]
        public string SessionName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

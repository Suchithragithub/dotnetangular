using System;
using System.ComponentModel.DataAnnotations;

namespace ModernApiProject.Domain.Entities
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdationDate { get; set; }
    }
}

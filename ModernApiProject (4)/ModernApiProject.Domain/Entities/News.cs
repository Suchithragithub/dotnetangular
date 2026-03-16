using System;
using System.ComponentModel.DataAnnotations;

namespace ModernApiProject.Domain.Entities
{
    public class News
    {
        [Key]
        public int Id { get; set; }
        public string Newstitle { get; set; }
        public string NewsDescription { get; set; }
        public DateTime PostingDate { get; set; }
    }
}

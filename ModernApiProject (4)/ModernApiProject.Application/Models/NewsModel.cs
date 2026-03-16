using System;

namespace ModernApiProject.Application.Models
{
    public class NewsModel
    {
        public int Id { get; set; }
        public string Newstitle { get; set; }
        public string NewsDescription { get; set; }
        public DateTime PostingDate { get; set; }
    }
}

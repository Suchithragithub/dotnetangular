using System;

namespace ModernApiProject.Application.Models
{
    public class SessionModel
    {
        public int Id { get; set; }
        public string SessionName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

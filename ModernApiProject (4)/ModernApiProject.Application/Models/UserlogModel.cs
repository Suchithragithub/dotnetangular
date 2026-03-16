using System;

namespace ModernApiProject.Application.Models
{
    public class UserlogModel
    {
        public int Id { get; set; }
        public string StudentRegno { get; set; }
        public byte[] Userip { get; set; }
        public DateTime LoginTime { get; set; }
        public string Logout { get; set; }
        public int Status { get; set; }
    }
}

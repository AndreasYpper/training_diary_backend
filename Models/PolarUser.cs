using System;

namespace training_diary_backend.Models
{
    public class PolarUser
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ExpiresIn { get; set; }
        public int PolarUserId { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}
using System.Collections.Generic;

namespace training_diary_backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public List<PolarWorkout> PolarWorkouts { get; set; }
        public PolarUser PolarUser { get; set; }
    }
}
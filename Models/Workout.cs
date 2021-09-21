namespace training_diary_backend.Models
{
    public class Workout
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Duration { get; set; }
        public int Distance { get; set; }

        public User User { get; set; }
    }
}
namespace training_diary_backend.Models
{
    public class PolarWorkout
    {
        public int Id { get; set; }
        public string ExerciseUrl { get; set; }

        public User User { get; set; }
    }
}
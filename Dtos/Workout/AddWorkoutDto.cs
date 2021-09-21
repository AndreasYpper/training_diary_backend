namespace training_diary_backend.Dtos.Workout
{
    public class AddWorkoutDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Duration { get; set; }
        public int Distance { get; set; }
    }
}
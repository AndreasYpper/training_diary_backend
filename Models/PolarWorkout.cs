namespace training_diary_backend.Models
{
    public class PolarWorkout
    {
        public int Id { get; set; }
        public int PolarId { get; set; }
        public string UploadTime { get; set; }
        public int TransactionId { get; set; }
        public string Device { get; set; }
        public string StartTime { get; set; }
        public int StartTimeUtcOffset { get; set; }
        public string Duration { get; set; }
        public int Calories { get; set; }
        public int Distance { get; set; }
        public int HeartRateAverage { get; set; }
        public int HeartRateMax { get; set; }
        public int TrainingLoad { get; set; }
        public string Sport { get; set; }
        public bool HasRoute { get; set; }
        public string DetailedSportInfo { get; set; }

        public User User { get; set; }
    }
}
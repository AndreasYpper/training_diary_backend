using Microsoft.EntityFrameworkCore;
using training_diary_backend.Models;

namespace training_diary_backend.Data
{
    public class DataContext :DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        public DbSet<Workout> Workouts { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
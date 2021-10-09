using Microsoft.EntityFrameworkCore;
using training_diary_backend.Models;

namespace training_diary_backend.Data
{
    public class DataContext :DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }

        public DbSet<PolarUser> PolarUsers { get; set; }
        public DbSet<PolarWorkout> PolarWorkouts { get; set; }
    }
}
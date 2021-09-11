using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.WorkoutService
{
    public class WorkoutService : IWorkoutService
    {
        private static List<Workout> workouts = new List<Workout> {
            new Workout{Id = 0, Title = "Cycling", Duration = 3600, Distance = 30000},
            new Workout{Id = 0, Title = "Running", Duration = 3600, Distance = 10000},
        };
        public async Task<ServiceResponse<List<Workout>>> GetAllWorkouts()
        {
            var serviceResponse = new ServiceResponse<List<Workout>>();
            serviceResponse.Data = workouts;
            serviceResponse.Success = true;
            serviceResponse.Message = "Success";
            return serviceResponse;
        }
    }
}
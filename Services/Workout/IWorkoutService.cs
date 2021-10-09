using System.Collections.Generic;
using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Workout
{
    public interface IWorkoutService
    {
         Task<ServiceResponse<List<PolarWorkout>>> GetAllWorkouts();
    }
}
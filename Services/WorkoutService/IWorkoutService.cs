using training_diary_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace training_diary_backend.Services.WorkoutService
{
    public interface IWorkoutService
    {
        Task<ServiceResponse<List<Workout>>> GetAllWorkouts();
    }
}
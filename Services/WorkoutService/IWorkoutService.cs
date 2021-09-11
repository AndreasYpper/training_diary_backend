using training_diary_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace training_diary_backend.Services.WorkoutService
{
    public interface IWorkoutService
    {
        Task<ServiceResponse<List<Workout>>> GetAllWorkouts();
        Task<ServiceResponse<Workout>> GetSingleWorkout(int id);
        Task<ServiceResponse<List<Workout>>> CreateWorkout(Workout newWorkout);
        Task<ServiceResponse<Workout>> UpdateWorkout(Workout updatedWorkout);
        Task<ServiceResponse<List<Workout>>> DeleteWorkout(int id);
    }
}
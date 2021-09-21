using training_diary_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using training_diary_backend.Dtos.Workout;

namespace training_diary_backend.Services.WorkoutService
{
    public interface IWorkoutService
    {
        Task<ServiceResponse<List<GetWorkoutDto>>> GetAllWorkouts();
        Task<ServiceResponse<GetWorkoutDto>> GetSingleWorkout(int id);
        Task<ServiceResponse<List<GetWorkoutDto>>> CreateWorkout(AddWorkoutDto newWorkout);
        Task<ServiceResponse<GetWorkoutDto>> UpdateWorkout(UpdateWorkoutDto updatedWorkout);
        Task<ServiceResponse<List<GetWorkoutDto>>> DeleteWorkout(int id);
    }
}
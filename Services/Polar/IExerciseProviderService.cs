using System.Collections.Generic;
using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public interface IExerciseProviderService
    {
        Task<ServiceResponse<string>> Authorize();
        Task<ServiceResponse<string>> Callback(string code);
        Task<ServiceResponse<string>> RegisterUser(string accessToken, int expiresIn, int xUserId, int userId);
        Task<ServiceResponse<string>> DeleteUser(string accessToken, int polarUserId);
        Task<ServiceResponse<string>> GetNewWorkouts(int polarUserId);
        Task<ServiceResponse<string>> GetWorkout(int workoutId, int polarUserId);
    }
}
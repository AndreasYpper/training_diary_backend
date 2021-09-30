using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public interface IProviderExercise
    {
         public Task<ServiceResponse<string>> CreateTransaction(PolarUser user);
         public Task<ServiceResponse<string>> GetTransactionWorkouts(int transactionId, string resourceUri, string token);
         public Task<ServiceResponse<string>> CommitTransaction(int transactionId, int polarUserId, string token);
         public Task<ServiceResponse<string>> GetWorkout(string exerciseUrl, string token);
    }
}
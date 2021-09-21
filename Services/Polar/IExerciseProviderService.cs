using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public interface IExerciseProviderService
    {
         Task<ServiceResponse<string>> Authorize();
         Task<ServiceResponse<string>> Callback(string code);
    }
}
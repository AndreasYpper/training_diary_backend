using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public interface IProviderUser
    {
         public ServiceResponse<string> GetAuthorizationUrl();
         public Task<ServiceResponse<string>> GetAccessToken(string _code);
         public Task<ServiceResponse<string>> RegisterUser(string accessToken, int userId);
         public Task<ServiceResponse<string>> UnregisterUser(string accessToken, int polarUserId);
    }
}
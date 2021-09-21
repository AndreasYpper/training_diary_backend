using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public class ExerciseProviderService : IExerciseProviderService
    {
        private readonly IConfiguration _config;
        public ExerciseProviderService(IConfiguration config)
        {
            _config = config;

        }
        public async Task<ServiceResponse<string>> Authorize()
        {
            var response = new ServiceResponse<string>();

            Oauth oauth = new Oauth(_config.GetSection("Polar:Url").Value.ToString(),
                _config.GetSection("Polar:Auth").Value.ToString(),
                _config.GetSection("Polar:Access").Value.ToString(),
                _config.GetSection("Polar:Redirect").Value.ToString(),
                _config.GetSection("Polar:Client_Id").Value.ToString(),
                _config.GetSection("Polar:Client_Secret").Value.ToString());

            AccessLink access = new AccessLink(oauth);
            access.ClientId = _config.GetSection("Polar:Client_Id").Value.ToString();
            access.ClientSecret = _config.GetSection("Polar:Client_Secret").Value.ToString();
            access.redirect_url = _config.GetSection("Polar:Redirect").Value.ToString();

            response.Data = access.authorization_url();
            return response;
        }

        public async Task<ServiceResponse<string>> Callback(string code)
        {
            Oauth oauth = new Oauth(_config.GetSection("Polar:Url").Value.ToString(),
                _config.GetSection("Polar:Auth").Value.ToString(),
                _config.GetSection("Polar:Access").Value.ToString(),
                _config.GetSection("Polar:Redirect").Value.ToString(),
                _config.GetSection("Polar:Client_Id").Value.ToString(),
                _config.GetSection("Polar:Client_Secret").Value.ToString());

            AccessLink access = new AccessLink(oauth);
            access.ClientId = _config.GetSection("Polar:Client_Id").Value.ToString();
            access.ClientSecret = _config.GetSection("Polar:Client_Secret").Value.ToString();
            access.redirect_url = _config.GetSection("Polar:Redirect").Value.ToString();

            string token_response = await access.get_access_token(code);

            var response = new ServiceResponse<string>();
            response.Data = token_response;

            return response;
        }
    }
}
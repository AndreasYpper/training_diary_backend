using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using training_diary_backend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using training_diary_backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace training_diary_backend.Services.Polar
{
    public class ExerciseProviderService : IExerciseProviderService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _context;
        public ExerciseProviderService(IConfiguration config, IHttpContextAccessor httpContextAccessor, DataContext context)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;

        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
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

            response.Data = await access.authorization_url();


            return response;
        }

        public async Task<ServiceResponse<string>> Callback(string code)
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

            response.Data = await access.get_access_token(code);

            return response;
        }

        public async Task<ServiceResponse<string>> RegisterUser(string accessToken, int expiresIn, int xUserId, int userId)
        {
            var response = new ServiceResponse<string>();

            response.Data = await RegisterUserCall(accessToken, userId);

            if (response.Data == "Conflict")
            {
                response.Success = false;
                response.Message = "User is already registered.";
            }

            else if (response.Data == "NoContent")
            {
                response.Success = false;
                response.Message = "User not found.";
            }

            else
            {
                User user = await _context.Users.FirstAsync(u => u.Id == userId);

                PolarUser polar = new PolarUser();
                polar.Token = accessToken;
                polar.ExpiresIn = expiresIn;
                polar.PolarUserId = xUserId;
                polar.UserId = userId;
                polar.User = user;

                _context.PolarUsers.Add(polar);
                await _context.SaveChangesAsync();

                response.Data = polar.Id.ToString();
            }

            return response;
        }

        public async Task<ServiceResponse<string>> DeleteUser(string accessToken, int polarUserId)
        {
            var response = new ServiceResponse<string>();

            response.Data = await UnregisterUser(accessToken, polarUserId);

            if (response.Data == "NoContent")
            {
                try
                {
                    PolarUser polar = await _context.PolarUsers.FirstAsync(p => p.PolarUserId == polarUserId);
                    _context.Remove(polar);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        private async Task<string> RegisterUserCall(string accessToken, int userId)
        {
            StringContent data_req = new StringContent("{\"member-id\": " + "\"" + userId + "\"}",
                                    System.Text.Encoding.UTF8,
                                    "application/json");
            data_req.Headers.ContentType.CharSet = "";
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BaseAddress = new Uri("https://www.polaraccesslink.com/v3/users");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.PostAsync(client.BaseAddress, data_req);

            return response.StatusCode.ToString();
        }

        private async Task<string> UnregisterUser(string accessToken, int polarUserId)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://www.polaraccesslink.com/v3/users/" + polarUserId);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.DeleteAsync(client.BaseAddress);

            return response.StatusCode.ToString();
        }
    }
}
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
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;

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

        public async Task<ServiceResponse<string>> Authorize()
        {
            var response = new ServiceResponse<string>();

            response.Data = get_authorization_url();

            response.Success = true;


            return response;
        }

        public async Task<ServiceResponse<string>> Callback(string code)
        {
            var response = new ServiceResponse<string>();

            response.Data = await get_access_token(code);

            response.Success = true;

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
                response.Success = true;
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

                    response.Success = true;
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        public async Task<ServiceResponse<List<string>>> GetNewWorkouts(int polarUserId)
        {
            var response = new ServiceResponse<List<string>>();

            PolarUser polar = await _context.PolarUsers.FirstOrDefaultAsync(p => p.PolarUserId == polarUserId);

            var client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetSection("Polar:Url").Value.ToString() + "/users/" + polarUserId + "/exercise-transactions");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", polar.Token);


            var polarResponse = await client.PostAsync(client.BaseAddress, new StringContent(""));

            if (polarResponse.StatusCode.ToString() == "NoContent")
            {
                response.Success = false;
                response.Message = "No new data.";
            }
            else
            {
                var result = await polarResponse.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(result);

                var test = await GetTransactionWorkouts(int.Parse(json["transaction-id"].ToString()), json["resource-uri"].ToString(), polar.Token);
                var test2 = await CommitTransaction(int.Parse(json["transaction-id"].ToString()), polar.PolarUserId, polar.Token);
            }

            return response;
        }
        private string get_authorization_url()
        {
            Dictionary<string, string> Params = new Dictionary<string, string>();

            Params.Add("client_id", _config.GetSection("Polar:Client_Id").Value.ToString());
            Params.Add("response_type", "code");
            Params.Add("redirect_url", _config.GetSection("Polar:Redirect").Value.ToString());

            return QueryHelpers.AddQueryString(_config.GetSection("Polar:Auth").Value.ToString(), Params);
        }

        private async Task<string> get_access_token(string _code)
        {
            StringContent data_req = new StringContent("grant_type=authorization_code&code=" + _code, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            data_req.Headers.ContentType.CharSet = "";

            string content = data_req.ReadAsStringAsync().Result;

            var authToken = System.Text.Encoding.ASCII.GetBytes($"{_config.GetSection("Polar:Client_Id").Value.ToString()}:{_config.GetSection("Polar:Client_Secret").Value.ToString()}");

            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BaseAddress = new Uri(_config.GetSection("Polar:Access").Value.ToString() + "?grant_type=authorization_code&code=" + _code);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;charset=UTF-8");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            var response = await client.PostAsync(_config.GetSection("Polar:Access").Value.ToString(), data_req);

            string result = await response.Content.ReadAsStringAsync();

            return result;
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

        private async Task<ServiceResponse<string>> GetTransactionWorkouts(int transactionId, string resourceUri, string token)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();
            Console.WriteLine("GET TRANSACTION WORKOUTS");

            client.BaseAddress = new Uri(resourceUri);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var clientResponse = await client.GetAsync(client.BaseAddress);

            Console.WriteLine(clientResponse);

            var result = await clientResponse.Content.ReadAsStringAsync();

            Console.WriteLine(result);
            return response;
        }

        private async Task<string> CommitTransaction(int transactionId, int polarUserId, string token)
        {
            var client = new HttpClient();

            Console.WriteLine("COMMIT TRANSACTIONS");
            Console.WriteLine(token);

            client.BaseAddress = new Uri(_config.GetSection("Polar:Url").Value.ToString() + "users/" + polarUserId + "/exercise-transactions/" + transactionId);
            Console.WriteLine(client.BaseAddress);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsync(client.BaseAddress, new StringContent(""));
            Console.WriteLine(response);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

            return string.Empty;
        }
    }
}
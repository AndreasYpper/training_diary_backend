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
        private readonly IProviderUser _providerUser;
        public ExerciseProviderService(IConfiguration config, IHttpContextAccessor httpContextAccessor, DataContext context, IProviderUser providerUser)
        {
            _providerUser = providerUser;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;

        }

        public async Task<ServiceResponse<string>> Authorize()
        {
            var response = new ServiceResponse<string>();

            response = _providerUser.GetAuthorizationUrl();

            return response;
        }

        public async Task<ServiceResponse<string>> Callback(string code)
        {
            var response = new ServiceResponse<string>();

            response = await _providerUser.GetAccessToken(code);

            return response;
        }

        public async Task<ServiceResponse<string>> RegisterUser(string accessToken, int expiresIn, int xUserId, int userId)
        {
            var response = new ServiceResponse<string>();

            response = await _providerUser.RegisterUser(accessToken, userId);

            if (response.Success)
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

            response = await _providerUser.UnregisterUser(accessToken, polarUserId);

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

        public async Task<ServiceResponse<string>> GetNewWorkouts(int polarUserId)
        {
            var response = new ServiceResponse<string>();

            PolarUser polar = await _context.PolarUsers.FirstOrDefaultAsync(p => p.PolarUserId == polarUserId);
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == polar.UserId);

            var transaction = await CreateTransaction(polar);

            if (!transaction.Success)
            {
                response = transaction;
            }
            else
            {
                JObject transactionJson = JObject.Parse(transaction.Data);

                var exercises = await GetTransactionWorkouts(int.Parse(transactionJson["transaction-id"].ToString()), transactionJson["resource-uri"].ToString(), polar.Token);

                if (!exercises.Success)
                {
                    response = exercises;
                }

                else
                {
                    var commitTransaction = await CommitTransaction(int.Parse(transactionJson["transaction-id"].ToString()), polar.PolarUserId, polar.Token);
                    if (!commitTransaction.Success)
                    {
                        response = commitTransaction;
                    }

                    else
                    {
                        response = exercises;
                        JObject exerciseJson = JObject.Parse(exercises.Data);
                        foreach (var exercise in exerciseJson["exercises"])
                        {
                            PolarWorkout workout = new PolarWorkout();
                            workout.ExerciseUrl = exercise.ToString();
                            workout.User = user;

                            _context.PolarWorkouts.Add(workout);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return response;
        }

        public async Task<ServiceResponse<string>> GetWorkout(int workoutId, int polarUserId)
        {
            var response = new ServiceResponse<string>();

            try
            {
                PolarWorkout workout = await _context.PolarWorkouts.FirstAsync(w => w.Id == workoutId);
                PolarUser polar = await _context.PolarUsers.FirstAsync(p => p.Id == polarUserId);

                var workoutData = await GetWorkoutCall(workout.ExerciseUrl, polar.Token);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }


            return response;
        }

        private async Task<ServiceResponse<string>> GetWorkoutCall(string exerciseUrl, string token)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();
            client.BaseAddress = new Uri(exerciseUrl);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine(client.DefaultRequestHeaders);

            var polarResponse = await client.GetAsync(client.BaseAddress);
            Console.WriteLine(polarResponse);

            if (polarResponse.StatusCode.ToString() == "NoContent")
            {
                response.Success = false;
                response.Message = "No content";
            }
            else if (polarResponse.StatusCode.ToString() == "OK")
            {
                string result = await polarResponse.Content.ReadAsStringAsync();
                response.Data = result;
                response.Success = true;
            }

            else
            {
                response.Success = false;
                response.Message = polarResponse.StatusCode.ToString();
                string result = await polarResponse.Content.ReadAsStringAsync();
                response.Data = result;
            }

            return response;
        }

        

        private async Task<ServiceResponse<string>> CreateTransaction(PolarUser user)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetSection("Polar:Url").Value.ToString() + "/users/" + user.PolarUserId + "/exercise-transactions");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);


            var polarResponse = await client.PostAsync(client.BaseAddress, new StringContent(""));

            if (polarResponse.StatusCode.ToString() == "NoContent")
            {
                response.Success = false;
                response.Message = "No new data.";
            }
            else
            {
                var result = await polarResponse.Content.ReadAsStringAsync();
                response.Data = result;
                response.Success = true;
            }

            return response;
        }

        private async Task<ServiceResponse<string>> GetTransactionWorkouts(int transactionId, string resourceUri, string token)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();

            client.BaseAddress = new Uri(resourceUri);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var polarResponse = await client.GetAsync(client.BaseAddress);

            if (polarResponse.StatusCode.ToString() == "NoContent")
            {
                response.Success = false;
                response.Message = "No data";
            }

            else
            {
                var result = await polarResponse.Content.ReadAsStringAsync();

                response.Data = result;
                response.Success = true;
            }

            return response;
        }

        private async Task<ServiceResponse<string>> CommitTransaction(int transactionId, int polarUserId, string token)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetSection("Polar:Url").Value.ToString() + "/users/" + polarUserId + "/exercise-transactions/" + transactionId);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var polarResponse = await client.PutAsync(client.BaseAddress, new StringContent(""));


            if (polarResponse.StatusCode.ToString() == "NoContent")
            {
                response.Success = false;
                response.Message = "No data";
            }
            else if (polarResponse.StatusCode.ToString() == "NotFound")
            {
                response.Success = false;
                response.Data = "Not found";
            }

            else
            {
                var result = await polarResponse.Content.ReadAsStringAsync();

                response.Data = result;
                response.Success = true;
            }

            return response;
        }
    }
}
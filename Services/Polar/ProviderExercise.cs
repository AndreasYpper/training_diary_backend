using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public class ProviderExercise : IProviderExercise
    {
        private readonly IConfiguration _config;
        public ProviderExercise(IConfiguration config)
        {
            _config = config;

        }
        public async Task<ServiceResponse<string>> CreateTransaction(PolarUser user)
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

        public async Task<ServiceResponse<string>> GetTransactionWorkouts(int transactionId, string resourceUri, string token)
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

        public async Task<ServiceResponse<string>> CommitTransaction(int transactionId, int polarUserId, string token)
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

        public async Task<ServiceResponse<string>> GetWorkout(string exerciseUrl, string token)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();
            client.BaseAddress = new Uri(exerciseUrl);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var polarResponse = await client.GetAsync(client.BaseAddress);

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
    }
}
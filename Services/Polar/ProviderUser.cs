using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Polar
{
    public class ProviderUser : IProviderUser
    {
        private readonly IConfiguration _config;

        public ProviderUser(IConfiguration config)
        {
            _config = config;

        }
        public ServiceResponse<string> GetAuthorizationUrl()
        {
            var response = new ServiceResponse<string>();

            Dictionary<string, string> Params = new Dictionary<string, string>();

            Params.Add("client_id", _config.GetSection("Polar:Client_Id").Value.ToString());
            Params.Add("response_type", "code");
            Params.Add("redirect_url", _config.GetSection("Polar:Redirect").Value.ToString());

            response.Data = QueryHelpers.AddQueryString(_config.GetSection("Polar:Auth").Value.ToString(), Params);
            response.Success = true;

            return response;
        }

        public async Task<ServiceResponse<string>> GetAccessToken(string _code)
        {
            var response = new ServiceResponse<string>();

            StringContent data_req = new StringContent("grant_type=authorization_code&code=" + _code, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            data_req.Headers.ContentType.CharSet = "";

            string content = data_req.ReadAsStringAsync().Result;

            var authToken = System.Text.Encoding.ASCII.GetBytes($"{_config.GetSection("Polar:Client_Id").Value.ToString()}:{_config.GetSection("Polar:Client_Secret").Value.ToString()}");

            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BaseAddress = new Uri(_config.GetSection("Polar:Access").Value.ToString() + "?grant_type=authorization_code&code=" + _code);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;charset=UTF-8");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            var polarResponse = await client.PostAsync(_config.GetSection("Polar:Access").Value.ToString(), data_req);

            if(polarResponse.StatusCode.ToString() == "BadRequest")
            {
                response.Success = false;
                response.Message = polarResponse.StatusCode.ToString();
            }
            else
            {
                string result = await polarResponse.Content.ReadAsStringAsync();
                response.Data = result;
                response.Success = true;
            }

            return response;
        }

        public async Task<ServiceResponse<string>> RegisterUser(string accessToken, int userId)
        {
            var response = new ServiceResponse<string>();

            StringContent data_req = new StringContent("{\"member-id\": " + "\"" + userId + "\"}",
                                    System.Text.Encoding.UTF8,
                                    "application/json");
            data_req.Headers.ContentType.CharSet = "";
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BaseAddress = new Uri("https://www.polaraccesslink.com/v3/users");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var polarResponse = await client.PostAsync(client.BaseAddress, data_req);

            if(polarResponse.StatusCode.ToString() == "Conflict")
            {
                response.Success = false;
                response.Message = "User does already exist.";
            }

            else if (polarResponse.StatusCode.ToString() == "NoContent")
            {
                response.Success = false;
                response.Message = "User not found.";
            }
            
            else if (polarResponse.StatusCode.ToString() == "OK")
            {
                response.Success = true;
            }
            
            else
            {
                response.Success = false;
                response.Message = polarResponse.StatusCode.ToString();
            }

            return response;
        }

        public async Task<ServiceResponse<string>> UnregisterUser(string accessToken, int polarUserId)
        {
            var response = new ServiceResponse<string>();

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://www.polaraccesslink.com/v3/users/" + polarUserId);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var polarResponse = await client.DeleteAsync(client.BaseAddress);

            response.Data = polarResponse.StatusCode.ToString();
            response.Success = true;

            return response;
        }
    }
}
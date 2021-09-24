using System;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace training_diary_backend.Services.Polar
{
    public class Oauth
    {
        public string Url { get; set; }
        public string AuthorizationUrl { get; set; }
        public string AccessTokenUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public Oauth(string url, string auth, string access, string redirect, string client, string secret)
        {
            Url = url;
            AuthorizationUrl = auth;
            AccessTokenUrl = access;
            RedirectUrl = redirect;
            ClientId = client;
            ClientSecret = secret;
        }
        public async Task<string> get_authorization_url()
        {
            Dictionary<string, string> Params = new Dictionary<string, string>();

            Params.Add("client_id", ClientId);
            Params.Add("response_type", "code");
            Params.Add("redirect_url", RedirectUrl);

            return QueryHelpers.AddQueryString(AuthorizationUrl, Params);
        }

        public async Task<string> get_access_token(string _code)
        {
            StringContent data_req = new StringContent("grant_type=authorization_code&code=" + _code, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            data_req.Headers.ContentType.CharSet = "";

            string content = data_req.ReadAsStringAsync().Result;

            var authToken = System.Text.Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}");

            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            client.BaseAddress = new Uri(AccessTokenUrl + "?grant_type=authorization_code&code=" + _code);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;charset=UTF-8");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            var response = await client.PostAsync(AccessTokenUrl, data_req);            

            string result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}
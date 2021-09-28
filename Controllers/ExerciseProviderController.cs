using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using training_diary_backend.Models;
using training_diary_backend.Services.Polar;

namespace training_diary_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExerciseProviderController : ControllerBase
    {
        private readonly IExerciseProviderService _provider;
        public ExerciseProviderController(IExerciseProviderService provider)
        {
            _provider = provider;
        }

        [HttpGet("Authorize")]
        public async Task<ActionResult<ServiceResponse<string>>> Authorize()
        {
            var response = await _provider.Authorize();

            return Ok(response);
        }

        [HttpGet("oauth2_callback")]
        public async Task<ActionResult> Callback(string code)
        {
            var response = await _provider.Callback(code);

            return Ok(response);
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(string accessToken, int expiresIn, int xUserId, int userId)
        {
            var response = await _provider.RegisterUser(accessToken, expiresIn, xUserId, userId);

            return Ok(response);
        }

        [HttpDelete("Unregister")]
        public async Task<ActionResult> Unregister(string accessToken, int polarUserId)
        {
            var response = await _provider.DeleteUser(accessToken, polarUserId);
            return Ok(response);
        }

        [HttpPost("New_workouts")]
        public async Task<ActionResult> NewWorkouts(int polarUserId)
        {
            var response = await _provider.GetNewWorkouts(polarUserId);

            return Ok(response);
        }
    }
}
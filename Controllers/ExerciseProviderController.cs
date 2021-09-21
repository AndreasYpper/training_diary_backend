using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            
            Console.WriteLine(response.Data);
            return Ok(response);
        }

        [HttpGet("oauth2_callback")]
        public async Task<ActionResult> Callback(string code)
        {
            var response = await _provider.Callback(code);

            return Ok();
        }
    }
}
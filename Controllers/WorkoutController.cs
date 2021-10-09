using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using training_diary_backend.Models;
using training_diary_backend.Services.Workout;

namespace training_diary_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;
        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }
        [HttpGet("workouts")]
        public async Task<ActionResult<ServiceResponse<List<PolarWorkout>>>> GetAllWorkouts()
        {
            var response = await _workoutService.GetAllWorkouts();

            return Ok(response);
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using training_diary_backend.Models;
using training_diary_backend.Services.WorkoutService;

namespace training_diary_backend.Controllers
{
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;

        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<Workout>>>> GetWorkouts()
        {
            return Ok(await _workoutService.GetAllWorkouts());
        }
    }
}
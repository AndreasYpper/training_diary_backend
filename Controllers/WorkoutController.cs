using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using training_diary_backend.Models;
using training_diary_backend.Services.WorkoutService;

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

        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<Workout>>>> GetWorkouts()
        {
            return Ok(await _workoutService.GetAllWorkouts());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<Workout>>> GetWorkout(int id)
        {
            var response = await _workoutService.GetSingleWorkout(id);
            if(response.Data == null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<Workout>>>> CreateWorkout(Workout newWorkout)
        {
            return Ok(await _workoutService.CreateWorkout(newWorkout));
        }

        [HttpPut]
        public async Task<ActionResult<ServiceResponse<Workout>>> UpdateWorkout(Workout updatedWorkout)
        {
            var response = await _workoutService.UpdateWorkout(updatedWorkout);
            if(response.Data == null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpDelete]
        public async Task<ActionResult<ServiceResponse<List<Workout>>>> DeleteWorkout(int id)
        {
            var response = await _workoutService.DeleteWorkout(id);
            if(response.Data == null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using training_diary_backend.Data;
using training_diary_backend.Models;

namespace training_diary_backend.Services.Workout
{
    public class WorkoutService : IWorkoutService
    {
        private readonly DataContext _context;
        public WorkoutService(DataContext context)
        {
            _context = context;

        }
        public async Task<ServiceResponse<List<PolarWorkout>>> GetAllWorkouts()
        {
            var response = new ServiceResponse<List<PolarWorkout>>();

            try
            {
                 var workouts = await _context.PolarWorkouts.ToListAsync();

                response.Data = workouts;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
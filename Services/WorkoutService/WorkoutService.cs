using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using training_diary_backend.Data;
using training_diary_backend.Dtos.Workout;
using training_diary_backend.Models;

namespace training_diary_backend.Services.WorkoutService
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public WorkoutService(IMapper mapper, DataContext context)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<List<GetWorkoutDto>>> CreateWorkout(AddWorkoutDto newWorkout)
        {
            var serviceResponse = new ServiceResponse<List<GetWorkoutDto>>();
            Workout workout = _mapper.Map<Workout>(newWorkout);
            
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();
            
            serviceResponse.Data = await _context.Workouts.Select(w => _mapper.Map<GetWorkoutDto>(w)).ToListAsync();

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetWorkoutDto>>> DeleteWorkout(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetWorkoutDto>>();
            try
            {
                Workout workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id);
                _context.Workouts.Remove(workout);
                await _context.SaveChangesAsync();
                serviceResponse.Data = await _context.Workouts.Select(w => _mapper.Map<GetWorkoutDto>(w)).ToListAsync();
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetWorkoutDto>>> GetAllWorkouts()
        {
            var serviceResponse = new ServiceResponse<List<GetWorkoutDto>>();
            var dbWorkouts = await _context.Workouts.ToListAsync();
            serviceResponse.Data = dbWorkouts.Select(w => _mapper.Map<GetWorkoutDto>(w)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetWorkoutDto>> GetSingleWorkout(int id)
        {
            var serviceResponse = new ServiceResponse<GetWorkoutDto>();
            Workout workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id);
            serviceResponse.Data = _mapper.Map<GetWorkoutDto>(workout);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetWorkoutDto>> UpdateWorkout(UpdateWorkoutDto updatedWorkout)
        {
            var serviceResponse = new ServiceResponse<GetWorkoutDto>();

            try
            {
                Workout workout = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == updatedWorkout.Id);
                workout.Title = updatedWorkout.Title;
                workout.Content = updatedWorkout.Content;
                workout.Duration = updatedWorkout.Duration;
                workout.Distance = updatedWorkout.Distance;

                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetWorkoutDto>(workout);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using training_diary_backend.Models;

namespace training_diary_backend.Services.WorkoutService
{
    public class WorkoutService : IWorkoutService
    {
        private static List<Workout> workouts = new List<Workout> {
            new Workout{Id = 0, Title = "Cycling", Content = "This is content", Duration = 3600, Distance = 30000},
            new Workout{Id = 1, Title = "Running", Content = "This is content", Duration = 3600, Distance = 10000},
        };

        public async Task<ServiceResponse<List<Workout>>> CreateWorkout(Workout newWorkout)
        {
            var serviceResponse = new ServiceResponse<List<Workout>>();
            newWorkout.Id = workouts.Max(w => w.Id) + 1;

            workouts.Add(newWorkout);
            serviceResponse.Data = workouts;
            serviceResponse.Success = true;
            serviceResponse.Message = "Success";

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<Workout>>> DeleteWorkout(int id)
        {
            var serviceResponse = new ServiceResponse<List<Workout>>();
            try
            {
                Workout workout = workouts.FirstOrDefault(w => w.Id == id);
                workouts.Remove(workout);
                serviceResponse.Data = workouts;
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<Workout>>> GetAllWorkouts()
        {
            var serviceResponse = new ServiceResponse<List<Workout>>();
            serviceResponse.Data = workouts;
            serviceResponse.Success = true;
            serviceResponse.Message = "Success";
            return serviceResponse;
        }

        public async Task<ServiceResponse<Workout>> GetSingleWorkout(int id)
        {
            var serviceResponse = new ServiceResponse<Workout>();

            try
            {
                Workout workout = workouts.FirstOrDefault(w => w.Id == id);
                serviceResponse.Data = workout;

            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<Workout>> UpdateWorkout(Workout updatedWorkout)
        {
            var serviceResponse = new ServiceResponse<Workout>();

            try
            {
                Workout workout = workouts.FirstOrDefault(w => w.Id == updatedWorkout.Id);
                workout.Title = updatedWorkout.Title;
                workout.Content = updatedWorkout.Content;
                workout.Duration = updatedWorkout.Duration;
                workout.Distance = updatedWorkout.Distance;

                serviceResponse.Data = workout;
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
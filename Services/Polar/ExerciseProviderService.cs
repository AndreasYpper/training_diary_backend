using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using training_diary_backend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using training_diary_backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;

namespace training_diary_backend.Services.Polar
{
    public class ExerciseProviderService : IExerciseProviderService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _context;
        private readonly IProviderUser _providerUser;
        private readonly IProviderExercise _providerExercise;

        public ExerciseProviderService(IConfiguration config,
                                       IHttpContextAccessor httpContextAccessor,
                                       DataContext context,
                                       IProviderUser providerUser,
                                       IProviderExercise providerExercise)
        {
            _providerUser = providerUser;
            _providerExercise = providerExercise;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;

        }

        public async Task<ServiceResponse<string>> Authorize()
        {
            var response = new ServiceResponse<string>();

            response = _providerUser.GetAuthorizationUrl();

            return response;
        }

        public async Task<ServiceResponse<string>> Callback(string code)
        {
            var response = new ServiceResponse<string>();

            response = await _providerUser.GetAccessToken(code);

            return response;
        }

        public async Task<ServiceResponse<string>> RegisterUser(string accessToken, int expiresIn, int xUserId, int userId)
        {
            var response = new ServiceResponse<string>();

            response = await _providerUser.RegisterUser(accessToken, userId);

            if (response.Success)
            {
                User user = await _context.Users.FirstAsync(u => u.Id == userId);

                PolarUser polar = new PolarUser();
                polar.Token = accessToken;
                polar.ExpiresIn = expiresIn;
                polar.PolarUserId = xUserId;
                polar.UserId = userId;
                polar.User = user;

                _context.PolarUsers.Add(polar);
                await _context.SaveChangesAsync();

                response.Data = polar.Id.ToString();
            }

            return response;
        }

        public async Task<ServiceResponse<string>> DeleteUser(string accessToken, int polarUserId)
        {
            var response = new ServiceResponse<string>();

            response = await _providerUser.UnregisterUser(accessToken, polarUserId);

            if (response.Data == "NoContent")
            {
                try
                {
                    PolarUser polar = await _context.PolarUsers.FirstAsync(p => p.PolarUserId == polarUserId);
                    _context.Remove(polar);
                    await _context.SaveChangesAsync();

                    response.Success = true;
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        public async Task<ServiceResponse<string>> GetNewWorkouts(int polarUserId)
        {
            var response = new ServiceResponse<string>();

            PolarUser polar = await _context.PolarUsers.FirstOrDefaultAsync(p => p.PolarUserId == polarUserId);
            User user = await _context.Users.FirstAsync(u => u.Id == polar.UserId);

            var transaction = await _providerExercise.CreateTransaction(polar);

            if (!transaction.Success)
            {
                response = transaction;
            }
            else
            {
                JObject transactionJson = JObject.Parse(transaction.Data);

                var exercises = await _providerExercise.GetTransactionWorkouts(int.Parse(transactionJson["transaction-id"].ToString()), transactionJson["resource-uri"].ToString(), polar.Token);

                if (!exercises.Success)
                {
                    response = exercises;
                }

                else
                {
                    response = exercises;
                    JObject exerciseJson = JObject.Parse(exercises.Data);
                    foreach (var exercise in exerciseJson["exercises"])
                    {
                        try
                        {
                            var exerciseData = await _providerExercise.GetWorkout(exercise.ToString(), polar.Token);
                            JObject exerciseDataJson = JObject.Parse(exerciseData.Data);

                            PolarWorkout workout = new PolarWorkout();
                            workout.PolarId = exerciseDataJson["id"] != null ? int.Parse(exerciseDataJson["id"].ToString()) : 0;
                            workout.UploadTime = exerciseDataJson["upload-time"] != null ? exerciseDataJson["upload-time"].ToString() : "";
                            workout.TransactionId = exerciseDataJson["transaction-id"] != null ? int.Parse(exerciseDataJson["transaction-id"].ToString()) : 0;
                            workout.Device = exerciseDataJson["device"] != null ? exerciseDataJson["device"].ToString() : "";
                            workout.StartTime = exerciseDataJson["start-time"] != null ? exerciseDataJson["start-time"].ToString() : "";
                            workout.StartTimeUtcOffset = exerciseDataJson["start-time-utc-offset"] != null ? int.Parse(exerciseDataJson["start-time-utc-offset"].ToString()) : 0;
                            workout.Duration = exerciseDataJson["duration"] != null ? exerciseDataJson["duration"].ToString() : "";
                            workout.Calories = exerciseDataJson["calories"] != null ? int.Parse(exerciseDataJson["calories"].ToString()) : 0;
                            workout.Distance = exerciseDataJson["distance"] != null ? int.Parse(exerciseDataJson["distance"].ToString()) : 0;
                            workout.HeartRateAverage = exerciseDataJson["heart-rate"]["average"] != null ? int.Parse(exerciseDataJson["heart-rate"]["average"].ToString()) : 0;
                            workout.HeartRateMax = exerciseDataJson["heart-rate"]["maximum"] != null ? int.Parse(exerciseDataJson["heart-rate"]["maximum"].ToString()) : 0;
                            workout.TrainingLoad = exerciseDataJson["training-load"] != null ? int.Parse(exerciseDataJson["training-load"].ToString()) : 0;
                            workout.Sport = exerciseDataJson["sport"] != null ? exerciseDataJson["sport"].ToString() : "";
                            workout.HasRoute = exerciseDataJson["has-route"] != null ? exerciseDataJson["has-route"].Value<bool>() : false;
                            workout.DetailedSportInfo = exerciseDataJson["detailed-sport-info"] != null ? exerciseDataJson["detailed-sport-info"].ToString() : "";
                            workout.User = user;

                            _context.PolarWorkouts.Add(workout);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            response.Message = ex.Message;
                        }
                    }

                    var commitTransaction = await _providerExercise.CommitTransaction(int.Parse(transactionJson["transaction-id"].ToString()), polar.PolarUserId, polar.Token);
                    if (!commitTransaction.Success)
                    {
                        response = commitTransaction;
                    }
                }
            }

            return response;
        }

        public Task<ServiceResponse<string>> GetWorkout(int workoutId, int polarUserId)
        {
            throw new NotImplementedException();
        }

        // public async Task<ServiceResponse<string>> GetWorkout(int workoutId, int polarUserId)
        // {
        //     var response = new ServiceResponse<string>();

        //     try
        //     {
        //         PolarWorkout workout = await _context.PolarWorkouts.FirstAsync(w => w.Id == workoutId);
        //         PolarUser polar = await _context.PolarUsers.FirstAsync(p => p.Id == polarUserId);

        //         var workoutData = await _providerExercise.GetWorkoutCall(workout.ExerciseUrl, polar.Token);
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Success = false;
        //         response.Message = ex.Message;
        //     }


        //     return response;
        // }
    }
}
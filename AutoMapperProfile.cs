using AutoMapper;
using training_diary_backend.Dtos.Workout;
using training_diary_backend.Models;

namespace dotNET_rpg
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Workout, GetWorkoutDto>();
            CreateMap<AddWorkoutDto, Workout>();
        }
    }
}
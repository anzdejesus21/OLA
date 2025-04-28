using AutoMapper;
using OLA.Entities;
using OLA.Models;

namespace OLA.Utilities
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserModel>().ReverseMap();
        }
    }
}

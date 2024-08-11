using AutoMapper;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web
{
    public class MappingConfigFile : Profile
    {
        public MappingConfigFile() {
            CreateMap<VillaDTO, VillaCreateDTO>().ReverseMap();
            CreateMap<VillaDTO, VillaUpdateDTO>().ReverseMap();

            CreateMap<VillaNumberDTO, VillaNumberCreateDTO>().ReverseMap();
            CreateMap<VillaNumberDTO, VillaNumberUpdateDTO>().ReverseMap();
        }
    }
}

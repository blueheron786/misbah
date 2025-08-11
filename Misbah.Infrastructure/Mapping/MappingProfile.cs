using AutoMapper;
using Misbah.Application.DTOs;
using Misbah.Domain.Entities;

namespace Misbah.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Note mappings
            CreateMap<Note, NoteDto>();
            CreateMap<NoteDto, Note>();

            // FolderNode mappings
            CreateMap<FolderNode, FolderNodeDto>()
                .ForMember(dest => dest.Folders, opt => opt.MapFrom(src => src.Folders))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));
                
            CreateMap<FolderNodeDto, FolderNode>()
                .ForMember(dest => dest.Folders, opt => opt.MapFrom(src => src.Folders))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));
        }
    }
}

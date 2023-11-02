using AutoMapper;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Helpers
{
    /// <summary>
    /// AutoMapper profiles for mapping between domain models and DTOs.
    /// </summary>
    public class AutoMapperProfiles : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperProfiles"/> class.
        /// Configures AutoMapper mappings for various domain models and DTOs.
        /// </summary>
        public AutoMapperProfiles()
        {
            // Mapping between ChatApplicationUser and UserResponseDto in both directions.
            CreateMap<ChatApplicationUser, UserResponseDto>().ReverseMap();


            // Mapping between ChatApplicationUser and UserChatResponseDto with custom member mapping.
            CreateMap<ChatApplicationUser, UserChatResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ReverseMap();

            // Mapping between UnreadMessageCount and UserChatResponseDto with custom member mapping.
            CreateMap<UnreadMessageCount, UserChatResponseDto>()
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.MessageCount))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead));


            // Mapping between Message and MessageResponseDto in both directions.
            CreateMap<Message, MessageResponseDto>()
                .ForMember(dest => dest.GifUrl, opt => opt.MapFrom(src => "https://localhost:44394/Upload/" + src.GifData.GifName));
        }
    }
}

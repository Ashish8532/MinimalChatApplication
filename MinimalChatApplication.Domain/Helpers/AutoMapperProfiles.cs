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
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<ChatApplicationUser, UserResponseDto>().ReverseMap();

            CreateMap<ChatApplicationUser, UserChatResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            // Add other mappings as needed
            .ReverseMap(); // If you want to support reverse mapping as well

            // If you have additional mappings for UnreadMessageCount to UserChatResponseDto, add them here.
            CreateMap<UnreadMessageCount, UserChatResponseDto>()
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.MessageCount))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead));

            CreateMap<Message, MessageResponseDto>().ReverseMap();
        }
    }
}

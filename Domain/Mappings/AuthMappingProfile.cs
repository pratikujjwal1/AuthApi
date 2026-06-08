using AuthApi.Domain.DTOs;
using AuthApi.Domain.Entities;
using AutoMapper;

namespace AuthApi.Domain.Mappings
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            // SignUp: Request DTO → Domain Entity
            //CreateMap<SignUpRequestDto, User>()
            //    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            //    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            //    .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => "User"));
            CreateMap<SignUpRequestDto, User>()
                 .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                 .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)); // Role from request

            // Domain Entity → SignUpResponseDto
            CreateMap<User, SignUpResponseDto>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(_ => "User registered successfully."));

            // Domain Entity → LoginResponseDto
            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.TokenExpiry, opt => opt.Ignore());

            // Domain Entity → UserResponseDto (used in CRUD)
            CreateMap<User, UserResponseDto>();

            // AdminUpdateUserRequestDto → User (partial update)
            CreateMap<AdminUpdateUserRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // UpdateProfileRequestDto → User (partial update)
            CreateMap<UpdateProfileRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}

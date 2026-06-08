using AuthApi.Domain.DTOs;

namespace AuthApi.Provider.Interfaces
{
    public interface IAuthProvider
    {
        Task<ApiResponseDto<SignUpResponseDto>> SignUpAsync(SignUpRequestDto request);
        Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    }
}

using AuthApi.Domain.DTOs;

namespace AuthApi.Provider.Interfaces
{
    public interface IUserProvider
    {
        // Admin CRUD
        Task<ApiResponseDto<IEnumerable<UserResponseDto>>> GetAllUsersAsync();
        Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(int id);
        Task<ApiResponseDto<UserResponseDto>> AdminUpdateUserAsync(int id, AdminUpdateUserRequestDto request);
        Task<ApiResponseDto<bool>> DeleteUserAsync(int id);

        // Self (logged-in user)
        Task<ApiResponseDto<UserResponseDto>> GetMyProfileAsync(int userId);
        Task<ApiResponseDto<UserResponseDto>> UpdateMyProfileAsync(int userId, UpdateProfileRequestDto request);
        Task<ApiResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    }
}

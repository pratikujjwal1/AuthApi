using AuthApi.Domain.DTOs;
using AuthApi.Provider.Interfaces;
using AuthApi.Repository.Interfaces;
using AutoMapper;

namespace AuthApi.Provider.Implementations
{
    public class UserProvider : IUserProvider
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;

        public UserProvider(IAuthRepository authRepository, IMapper mapper)
        {
            _authRepository = authRepository;
            _mapper = mapper;
        }

        // ─── Admin: Get all users ──────────────────────────────
        public async Task<ApiResponseDto<IEnumerable<UserResponseDto>>> GetAllUsersAsync()
        {
            var users = await _authRepository.GetAllUsersAsync();
            var result = _mapper.Map<IEnumerable<UserResponseDto>>(users);
            return ApiResponseDto<IEnumerable<UserResponseDto>>.Ok(result);
        }

        // ─── Admin: Get user by ID ─────────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(int id)
        {
            var user = await _authRepository.GetByIdAsync(id);
            if (user == null)
                return ApiResponseDto<UserResponseDto>.Fail($"User with ID {id} not found.");

            return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        // ─── Admin: Update any user ────────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> AdminUpdateUserAsync(int id, AdminUpdateUserRequestDto request)
        {
            var user = await _authRepository.GetByIdAsync(id);
            if (user == null)
                return ApiResponseDto<UserResponseDto>.Fail($"User with ID {id} not found.");

            // Check if email is taken by another user
            var emailTaken = await _authRepository.EmailExistsForOtherUserAsync(request.Email, id);
            if (emailTaken)
                return ApiResponseDto<UserResponseDto>.Fail("Email is already in use by another account.");

            // Map updated fields onto existing entity
            _mapper.Map(request, user);

            var updated = await _authRepository.UpdateUserAsync(user);
            return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(updated), "User updated successfully.");
        }

        // ─── Admin: Soft delete user ───────────────────────────
        public async Task<ApiResponseDto<bool>> DeleteUserAsync(int id)
        {
            var user = await _authRepository.GetByIdAsync(id);
            if (user == null)
                return ApiResponseDto<bool>.Fail($"User with ID {id} not found.");

            var deleted = await _authRepository.DeleteUserAsync(id);
            return deleted
                ? ApiResponseDto<bool>.Ok(true, "User deleted successfully.")
                : ApiResponseDto<bool>.Fail("Failed to delete user.");
        }

        // ─── Self: Get my profile ──────────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> GetMyProfileAsync(int userId)
        {
            var user = await _authRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponseDto<UserResponseDto>.Fail("User not found.");

            return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        // ─── Self: Update my profile ───────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> UpdateMyProfileAsync(int userId, UpdateProfileRequestDto request)
        {
            var user = await _authRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponseDto<UserResponseDto>.Fail("User not found.");

            var emailTaken = await _authRepository.EmailExistsForOtherUserAsync(request.Email, userId);
            if (emailTaken)
                return ApiResponseDto<UserResponseDto>.Fail("Email is already in use by another account.");

            _mapper.Map(request, user);

            var updated = await _authRepository.UpdateUserAsync(user);
            return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(updated), "Profile updated successfully.");
        }

        // ─── Self: Change password ─────────────────────────────
        public async Task<ApiResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _authRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponseDto<bool>.Fail("User not found.");

            // Verify current password
            bool isValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isValid)
                return ApiResponseDto<bool>.Fail("Current password is incorrect.");

            // Hash and save new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _authRepository.UpdateUserAsync(user);

            return ApiResponseDto<bool>.Ok(true, "Password changed successfully.");
        }
    }
}

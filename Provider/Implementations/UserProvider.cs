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
        private readonly ILogger<UserProvider> _logger;

        public UserProvider(IAuthRepository authRepository, IMapper mapper, ILogger<UserProvider> logger)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _logger = logger;

        }

        // ─── Admin: Get all users ──────────────────────────────
        public async Task<ApiResponseDto<IEnumerable<UserResponseDto>>> GetAllUsersAsync()
        {
            _logger.LogInformation("GetAllUsersAsync started.");

            try
            {
                var users = await _authRepository.GetAllUsersAsync();
                var result = _mapper.Map<IEnumerable<UserResponseDto>>(users);

                _logger.LogInformation("GetAllUsersAsync completed. Total users fetched: {Count}", result.Count());
                return ApiResponseDto<IEnumerable<UserResponseDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllUsersAsync failed.");
                throw;
            }
        }

        // ─── Admin: Get user by ID ─────────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(int id)
        {

            _logger.LogInformation("GetUserByIdAsync started for UserId: {UserId}", id);

            try
            {
                var user = await _authRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("GetUserByIdAsync: User not found for UserId: {UserId}", id);
                    return ApiResponseDto<UserResponseDto>.Fail($"User with ID {id} not found.");
                }

                _logger.LogInformation("GetUserByIdAsync completed for UserId: {UserId}", id);
                return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserByIdAsync failed for UserId: {UserId}", id);
                throw;
            }
        }

        // ─── Admin: Update any user ────────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> AdminUpdateUserAsync(int id, AdminUpdateUserRequestDto request)
        {
            _logger.LogInformation("AdminUpdateUserAsync started for UserId: {UserId}", id);

            try
            {
                var user = await _authRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("AdminUpdateUserAsync: User not found for UserId: {UserId}", id);
                    return ApiResponseDto<UserResponseDto>.Fail($"User with ID {id} not found.");
                }

                var emailTaken = await _authRepository.EmailExistsForOtherUserAsync(request.Email, id);
                if (emailTaken)
                {
                    _logger.LogWarning("AdminUpdateUserAsync: Email already in use for UserId: {UserId}, Email: {Email}", id, request.Email);
                    return ApiResponseDto<UserResponseDto>.Fail("Email is already in use by another account.");
                }

                _mapper.Map(request, user);

                var updated = await _authRepository.UpdateUserAsync(user);
                _logger.LogInformation("AdminUpdateUserAsync completed for UserId: {UserId}", id);
                return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(updated), "User updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminUpdateUserAsync failed for UserId: {UserId}", id);
                throw;
            }
        }

        // ─── Admin: Soft delete user ───────────────────────────
        public async Task<ApiResponseDto<bool>> DeleteUserAsync(int id)
        {
            _logger.LogInformation("DeleteUserAsync started for UserId: {UserId}", id);

            try
            {
                var user = await _authRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("DeleteUserAsync: User not found for UserId: {UserId}", id);
                    return ApiResponseDto<bool>.Fail($"User with ID {id} not found.");
                }

                var deleted = await _authRepository.DeleteUserAsync(id);
                if (deleted)
                {
                    _logger.LogInformation("DeleteUserAsync: User soft-deleted successfully for UserId: {UserId}", id);
                    return ApiResponseDto<bool>.Ok(true, "User deleted successfully.");
                }
                else
                {
                    _logger.LogWarning("DeleteUserAsync: Deletion returned 0 rows for UserId: {UserId}", id);
                    return ApiResponseDto<bool>.Fail("Failed to delete user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteUserAsync failed for UserId: {UserId}", id);
                throw;
            }
        }

        // ─── Self: Get my profile ──────────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> GetMyProfileAsync(int userId)
        {
            _logger.LogInformation("GetMyProfileAsync started for UserId: {UserId}", userId);

            try
            {
                var user = await _authRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("GetMyProfileAsync: User not found for UserId: {UserId}", userId);
                    return ApiResponseDto<UserResponseDto>.Fail("User not found.");
                }

                _logger.LogInformation("GetMyProfileAsync completed for UserId: {UserId}", userId);
                return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyProfileAsync failed for UserId: {UserId}", userId);
                throw;
            }

        }

        // ─── Self: Update my profile ───────────────────────────
        public async Task<ApiResponseDto<UserResponseDto>> UpdateMyProfileAsync(int userId, UpdateProfileRequestDto request)
        {
            _logger.LogInformation("UpdateMyProfileAsync started for UserId: {UserId}", userId);

            try
            {
                var user = await _authRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("UpdateMyProfileAsync: User not found for UserId: {UserId}", userId);
                    return ApiResponseDto<UserResponseDto>.Fail("User not found.");
                }

                var emailTaken = await _authRepository.EmailExistsForOtherUserAsync(request.Email, userId);
                if (emailTaken)
                {
                    _logger.LogWarning("UpdateMyProfileAsync: Email already in use for UserId: {UserId}, Email: {Email}", userId, request.Email);
                    return ApiResponseDto<UserResponseDto>.Fail("Email is already in use by another account.");
                }

                _mapper.Map(request, user);

                var updated = await _authRepository.UpdateUserAsync(user);
                _logger.LogInformation("UpdateMyProfileAsync completed for UserId: {UserId}", userId);
                return ApiResponseDto<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(updated), "Profile updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateMyProfileAsync failed for UserId: {UserId}", userId);
                throw;
            }
        }

        // ─── Self: Change password ─────────────────────────────
        public async Task<ApiResponseDto<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation("ChangePasswordAsync started for UserId: {UserId}", userId);

            try
            {
                var user = await _authRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("ChangePasswordAsync: User not found for UserId: {UserId}", userId);
                    return ApiResponseDto<bool>.Fail("User not found.");
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
                if (!isValid)
                {
                    _logger.LogWarning("ChangePasswordAsync: Incorrect current password for UserId: {UserId}", userId);
                    return ApiResponseDto<bool>.Fail("Current password is incorrect.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _authRepository.UpdateUserAsync(user);

                _logger.LogInformation("ChangePasswordAsync completed for UserId: {UserId}", userId);
                return ApiResponseDto<bool>.Ok(true, "Password changed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePasswordAsync failed for UserId: {UserId}", userId);
                throw;
            }
        }
    }
}

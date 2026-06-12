using AuthApi.Domain.DTOs;
using AuthApi.Provider.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]   // Any logged-in user
    public class UserController : ControllerBase
    {
        private readonly IUserProvider _userProvider;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserProvider userProvider, ILogger<UserController> logger)
        {
            _userProvider = userProvider;
            _logger = logger;

        }

        // Helper to get logged-in user's ID from JWT
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")!);

        // GET api/user/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            _logger.LogInformation("GetMyProfile request for UserId: {UserId}", userId);

            try
            {
                var result = await _userProvider.GetMyProfileAsync(userId);

                if (!result.Success)
                {
                    _logger.LogWarning("Profile not found for UserId: {UserId}", userId);
                    return NotFound(result);
                }

                _logger.LogInformation("Profile fetched successfully for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetMyProfile for UserId: {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        // PUT api/user/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto request)
        {
            var userId = GetUserId();
            _logger.LogInformation("UpdateMyProfile request for UserId: {UserId}", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateMyProfile validation failed for UserId: {UserId}", userId);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userProvider.UpdateMyProfileAsync(userId, request);

                if (!result.Success)
                {
                    _logger.LogWarning("UpdateMyProfile failed for UserId: {UserId}. Reason: {Message}", userId, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Profile updated successfully for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in UpdateMyProfile for UserId: {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }


        }

        // PUT api/user/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = GetUserId();
            _logger.LogInformation("ChangePassword request for UserId: {UserId}", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ChangePassword validation failed for UserId: {UserId}", userId);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userProvider.ChangePasswordAsync(userId, request);

                if (!result.Success)
                {
                    _logger.LogWarning("ChangePassword failed for UserId: {UserId}. Reason: {Message}", userId, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Password changed successfully for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in ChangePassword for UserId: {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}

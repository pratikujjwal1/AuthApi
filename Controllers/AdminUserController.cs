using AuthApi.Domain.DTOs;
using AuthApi.Provider.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]   // All endpoints in this controller require Admin role
    public class AdminUserController : ControllerBase
    {
        private readonly IUserProvider _userProvider;
        private readonly ILogger<AdminUserController> _logger;

        public AdminUserController(IUserProvider userProvider, ILogger<AdminUserController> logger)
        {
            _userProvider = userProvider;
            _logger = logger;

        }

        // GET api/admin/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Admin: GetAllUsers request received.");
            try
            {
                var result = await _userProvider.GetAllUsersAsync();
                _logger.LogInformation("Admin: GetAllUsers returned successfully.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Admin GetAllUsers.");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        // GET api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            
            _logger.LogInformation("Admin: GetUserById request for UserId: {UserId}", id);

            try
            {
                var result = await _userProvider.GetUserByIdAsync(id);

                if (!result.Success)
                {
                    _logger.LogWarning("Admin: User not found for UserId: {UserId}", id);
                    return NotFound(result);
                }

                _logger.LogInformation("Admin: GetUserById successful for UserId: {UserId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Admin GetUserById for UserId: {UserId}", id);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        // PUT api/admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserRequestDto request)
        {
            _logger.LogInformation("Admin: UpdateUser request for UserId: {UserId}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin: UpdateUser validation failed for UserId: {UserId}", id);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userProvider.AdminUpdateUserAsync(id, request);

                if (!result.Success)
                {
                    _logger.LogWarning("Admin: UpdateUser failed for UserId: {UserId}. Reason: {Message}", id, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Admin: UpdateUser successful for UserId: {UserId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Admin UpdateUser for UserId: {UserId}", id);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        // DELETE api/admin/users/{id}  (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            
            _logger.LogInformation("Admin: DeleteUser request for UserId: {UserId}", id);

            try
            {
                var result = await _userProvider.DeleteUserAsync(id);

                if (!result.Success)
                {
                    _logger.LogWarning("Admin: DeleteUser failed for UserId: {UserId}. Reason: {Message}", id, result.Message);
                    return NotFound(result);
                }

                _logger.LogInformation("Admin: DeleteUser successful for UserId: {UserId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Admin DeleteUser for UserId: {UserId}", id);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}

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

        public AdminUserController(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        // GET api/admin/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userProvider.GetAllUsersAsync();
            return Ok(result);
        }

        // GET api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _userProvider.GetUserByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // PUT api/admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userProvider.AdminUpdateUserAsync(id, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // DELETE api/admin/users/{id}  (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userProvider.DeleteUserAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}

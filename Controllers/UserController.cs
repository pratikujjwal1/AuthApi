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

        public UserController(IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        // Helper to get logged-in user's ID from JWT
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")!);

        // GET api/user/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _userProvider.GetMyProfileAsync(GetUserId());
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        // PUT api/user/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userProvider.UpdateMyProfileAsync(GetUserId(), request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // PUT api/user/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userProvider.ChangePasswordAsync(GetUserId(), request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}

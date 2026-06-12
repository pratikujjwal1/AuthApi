using AuthApi.Domain.DTOs;
using AuthApi.Provider.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthProvider _authProvider;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthProvider authProvider, ILogger<AuthController> logger)
        {
            _authProvider = authProvider;
            _logger = logger;

        }

        // POST api/auth/signup
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto request)
        {
            _logger.LogInformation("SignUp request received for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("SignUp validation failed for email: {Email}", request.Email);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authProvider.SignUpAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("SignUp failed for email: {Email}. Reason: {Message}", request.Email, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("SignUp successful for email: {Email}", request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during SignUp for email: {Email}", request.Email);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }


        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login request received for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login validation failed for email: {Email}", request.Email);
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authProvider.LoginAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("Login failed for email: {Email}. Reason: {Message}", request.Email, result.Message);
                    return Unauthorized(result);
                }

                _logger.LogInformation("Login successful for email: {Email}", request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during Login for email: {Email}", request.Email);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }

        }
    }
}

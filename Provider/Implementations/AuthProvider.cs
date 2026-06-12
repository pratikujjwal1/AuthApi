using AuthApi.Domain.DTOs;
using AuthApi.Domain.Entities;
using AuthApi.Provider.Interfaces;
using AuthApi.Repository.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Provider.Implementations
{
    public class AuthProvider : IAuthProvider
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthProvider> _logger;

        public AuthProvider(IAuthRepository authRepository, IMapper mapper, IConfiguration configuration, ILogger<AuthProvider> logger)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;

        }

        public async Task<ApiResponseDto<SignUpResponseDto>> SignUpAsync(SignUpRequestDto request)
        {

            _logger.LogInformation("SignUpAsync started for email: {Email}", request.Email);

            try
            {
                var emailExists = await _authRepository.EmailExistsAsync(request.Email);
                if (emailExists)
                {
                    _logger.LogWarning("SignUpAsync: Email already registered - {Email}", request.Email);
                    return ApiResponseDto<SignUpResponseDto>.Fail("Email is already registered.");
                }

                var user = _mapper.Map<User>(request);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var createdUser = await _authRepository.CreateUserAsync(user);
                var response = _mapper.Map<SignUpResponseDto>(createdUser);

                _logger.LogInformation("SignUpAsync completed successfully for email: {Email}", request.Email);
                return ApiResponseDto<SignUpResponseDto>.Ok(response, "User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignUpAsync failed for email: {Email}", request.Email);
                throw;
            }
        }

        public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
           
            _logger.LogInformation("LoginAsync started for email: {Email}", request.Email);

            try
            {
                var user = await _authRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("LoginAsync: No user found for email: {Email}", request.Email);
                    return ApiResponseDto<LoginResponseDto>.Fail("Invalid email or password.");
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!isValidPassword)
                {
                    _logger.LogWarning("LoginAsync: Invalid password attempt for email: {Email}", request.Email);
                    return ApiResponseDto<LoginResponseDto>.Fail("Invalid email or password.");
                }

                var (token, expiry) = GenerateJwtToken(user);

                var response = _mapper.Map<LoginResponseDto>(user);
                response.Token = token;
                response.TokenExpiry = expiry;

                _logger.LogInformation("LoginAsync successful for email: {Email}", request.Email);
                return ApiResponseDto<LoginResponseDto>.Ok(response, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed for email: {Email}", request.Email);
                throw;
            }
        }

        private (string token, DateTime expiry) GenerateJwtToken(User user)
        {
            _logger.LogDebug("Generating JWT token for UserId: {UserId}", user.Id);

            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"]!;
                var issuer = jwtSettings["Issuer"]!;
                var audience = jwtSettings["Audience"]!;
                var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiry,
                    signingCredentials: credentials
                );

                _logger.LogDebug("JWT token generated for UserId: {UserId}, Expiry: {Expiry}", user.Id, expiry);
                return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token generation failed for UserId: {UserId}", user.Id);
                throw;
            }
        }
    }
}

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

        public AuthProvider(IAuthRepository authRepository, IMapper mapper, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<ApiResponseDto<SignUpResponseDto>> SignUpAsync(SignUpRequestDto request)
        {
            var emailExists = await _authRepository.EmailExistsAsync(request.Email);
            if (emailExists)
                return ApiResponseDto<SignUpResponseDto>.Fail("Email is already registered.");

            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var createdUser = await _authRepository.CreateUserAsync(user);
            var response = _mapper.Map<SignUpResponseDto>(createdUser);

            return ApiResponseDto<SignUpResponseDto>.Ok(response, "User registered successfully.");
        }

        public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email);
            if (user == null)
                return ApiResponseDto<LoginResponseDto>.Fail("Invalid email or password.");

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValidPassword)
                return ApiResponseDto<LoginResponseDto>.Fail("Invalid email or password.");

            var (token, expiry) = GenerateJwtToken(user);

            var response = _mapper.Map<LoginResponseDto>(user);
            response.Token = token;
            response.TokenExpiry = expiry;

            return ApiResponseDto<LoginResponseDto>.Ok(response, "Login successful.");
        }

        private (string token, DateTime expiry) GenerateJwtToken(User user)
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
                new Claim(ClaimTypes.Role, user.Role),           // Role claim for authorization
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

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }
    }
}

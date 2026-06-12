using AuthApi.Domain.Entities;
using AuthApi.Repository.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AuthApi.Repository.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IConfiguration configuration, ILogger<AuthRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;

        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        // ─── Get user by email ─────────────────────────────────
        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogDebug("GetByEmailAsync called for email: {Email}", email);

            const string sql = @"
                SELECT Id, FullName, Email, PasswordHash, CreatedAt, IsActive, Role
                FROM Users WHERE Email = @Email AND IsActive = 1";

            try
            {
                using var conn = CreateConnection();
                var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
                _logger.LogDebug("GetByEmailAsync result for email: {Email} - Found: {Found}", email, user != null);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByEmailAsync failed for email: {Email}", email);
                throw;
            }
        }

        // ─── Create user ───────────────────────────────────────
        public async Task<User> CreateUserAsync(User user)
        {
            _logger.LogDebug("CreateUserAsync called for email: {Email}", user.Email);

            const string sql = @"
                INSERT INTO Users (FullName, Email, PasswordHash, CreatedAt, IsActive, Role)
                OUTPUT INSERTED.*
                VALUES (@FullName, @Email, @PasswordHash, @CreatedAt, @IsActive, @Role)";

            try
            {
                using var conn = CreateConnection();
                var createdUser = await conn.QueryFirstAsync<User>(sql, user);
                _logger.LogInformation("CreateUserAsync: User created with Id: {UserId}", createdUser.Id);
                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateUserAsync failed for email: {Email}", user.Email);
                throw;
            }
        }

        // ─── Email exists ──────────────────────────────────────
        public async Task<bool> EmailExistsAsync(string email)
        {
            _logger.LogDebug("EmailExistsAsync called for email: {Email}", email);

            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            try
            {
                using var conn = CreateConnection();
                var exists = await conn.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
                _logger.LogDebug("EmailExistsAsync result for email: {Email} - Exists: {Exists}", email, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailExistsAsync failed for email: {Email}", email);
                throw;
            }
        }

        // ─── Email exists for another user (update check) ──────
        public async Task<bool> EmailExistsForOtherUserAsync(string email, int excludeUserId)
        {
            _logger.LogDebug("EmailExistsForOtherUserAsync called for email: {Email}, ExcludeUserId: {UserId}", email, excludeUserId);

            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND Id != @ExcludeUserId";

            try
            {
                using var conn = CreateConnection();
                var exists = await conn.ExecuteScalarAsync<int>(sql, new { Email = email, ExcludeUserId = excludeUserId }) > 0;
                _logger.LogDebug("EmailExistsForOtherUserAsync result: {Exists}", exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailExistsForOtherUserAsync failed for email: {Email}", email);
                throw;
            }
        }

        // ─── Get all users ─────────────────────────────────────
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogDebug("GetAllUsersAsync called.");

            const string sql = @"
                SELECT Id, FullName, Email, CreatedAt, IsActive, Role
                FROM Users ORDER BY CreatedAt DESC";

            try
            {
                using var conn = CreateConnection();
                var users = await conn.QueryAsync<User>(sql);
                _logger.LogDebug("GetAllUsersAsync returned {Count} users.", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllUsersAsync failed.");
                throw;
            }
        }

        // ─── Get user by ID ────────────────────────────────────
        public async Task<User?> GetByIdAsync(int id)
        {
            _logger.LogDebug("GetByIdAsync called for UserId: {UserId}", id);

            const string sql = @"
                SELECT Id, FullName, Email, PasswordHash, CreatedAt, IsActive, Role
                FROM Users WHERE Id = @Id";

            try
            {
                using var conn = CreateConnection();
                var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
                _logger.LogDebug("GetByIdAsync result for UserId: {UserId} - Found: {Found}", id, user != null);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed for UserId: {UserId}", id);
                throw;
            }
        }

        // ─── Update user ───────────────────────────────────────
        public async Task<User> UpdateUserAsync(User user)
        {
            _logger.LogDebug("UpdateUserAsync called for UserId: {UserId}", user.Id);

            const string sql = @"
                UPDATE Users SET
                    FullName     = @FullName,
                    Email        = @Email,
                    PasswordHash = @PasswordHash,
                    IsActive     = @IsActive,
                    Role         = @Role
                WHERE Id = @Id;

                SELECT Id, FullName, Email, PasswordHash, CreatedAt, IsActive, Role
                FROM Users WHERE Id = @Id";

            try
            {
                using var conn = CreateConnection();
                var updated = await conn.QueryFirstAsync<User>(sql, user);
                _logger.LogInformation("UpdateUserAsync: User updated successfully for UserId: {UserId}", user.Id);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUserAsync failed for UserId: {UserId}", user.Id);
                throw;
            }
        }

        // ─── Delete user (soft delete) ─────────────────────────
        public async Task<bool> DeleteUserAsync(int id)
        {
            _logger.LogDebug("DeleteUserAsync called for UserId: {UserId}", id);

            const string sql = "UPDATE Users SET IsActive = 0 WHERE Id = @Id";

            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new { Id = id });
                var success = rows > 0;
                _logger.LogInformation("DeleteUserAsync: UserId: {UserId} - Rows affected: {Rows}", id, rows);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteUserAsync failed for UserId: {UserId}", id);
                throw;
            }
        }
    }
}

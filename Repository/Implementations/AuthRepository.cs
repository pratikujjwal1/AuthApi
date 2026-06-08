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

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        // ─── Get user by email ─────────────────────────────────
        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT Id, FullName, Email, PasswordHash, CreatedAt, IsActive, Role
                FROM Users WHERE Email = @Email AND IsActive = 1";

            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        // ─── Create user ───────────────────────────────────────
        public async Task<User> CreateUserAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users (FullName, Email, PasswordHash, CreatedAt, IsActive, Role)
                OUTPUT INSERTED.*
                VALUES (@FullName, @Email, @PasswordHash, @CreatedAt, @IsActive, @Role)";

            using var conn = CreateConnection();
            return await conn.QueryFirstAsync<User>(sql, user);
        }

        // ─── Email exists ──────────────────────────────────────
        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
        }

        // ─── Email exists for another user (update check) ──────
        public async Task<bool> EmailExistsForOtherUserAsync(string email, int excludeUserId)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND Id != @ExcludeUserId";
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, new { Email = email, ExcludeUserId = excludeUserId }) > 0;
        }

        // ─── Get all users ─────────────────────────────────────
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string sql = @"
                SELECT Id, FullName, Email, CreatedAt, IsActive, Role
                FROM Users ORDER BY CreatedAt DESC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<User>(sql);
        }

        // ─── Get user by ID ────────────────────────────────────
        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, FullName, Email, PasswordHash, CreatedAt, IsActive, Role
                FROM Users WHERE Id = @Id";

            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        // ─── Update user ───────────────────────────────────────
        public async Task<User> UpdateUserAsync(User user)
        {
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

            using var conn = CreateConnection();
            return await conn.QueryFirstAsync<User>(sql, user);
        }

        // ─── Delete user (soft delete) ─────────────────────────
        public async Task<bool> DeleteUserAsync(int id)
        {
            const string sql = "UPDATE Users SET IsActive = 0 WHERE Id = @Id";
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }
}

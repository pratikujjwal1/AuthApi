using AuthApi.Domain.Entities;

namespace AuthApi.Repository.Interfaces
{
    public interface IAuthRepository
    {
        // Auth
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<bool> EmailExistsAsync(string email);

        // CRUD
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> EmailExistsForOtherUserAsync(string email, int excludeUserId);
    }
}

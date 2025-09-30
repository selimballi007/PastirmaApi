using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);

       
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);

        
        Task<User> GetByIdAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
    }
}

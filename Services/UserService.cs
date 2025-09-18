using Microsoft.EntityFrameworkCore;
using PastirmaApi.Data;
using PastirmaApi.Models;
using System.ComponentModel;
using static PastirmaApi.Controllers.UserController;

namespace PastirmaApi.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;  
        }

        public async Task<User> RegisterUserAsync(RegisterUserDTO dto) {
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = dto.PasswordHash,
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistAsync(string email)
        {           
            return await _context.Users.AnyAsync(u => u.Email == email);
        } 
    }
}

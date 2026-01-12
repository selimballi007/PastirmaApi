using Microsoft.EntityFrameworkCore;
using Npgsql;
using PastirmaApi.Application.DTOs.ReviewDTOs;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Repositories;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;

namespace PastirmaApi.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<LoginGetUserDTO?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new LoginGetUserDTO
                (
                    u.Id,
                    u.Username!,
                    u.Email,
                    u.Role,
                    u.PasswordHash!,
                    u.LastLoginAt,
                    u.RefreshToken,
                    u.RefreshTokenExpiry,
                    u.IsVerified,
                    u.FailedLoginAttempts,
                    u.LockoutEnd
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<UserRefreshTokenProjection?> GetByRefreshTokenAsync(string refreshToken, int userId)
        {
            return await _context.Users
                .Where(u => u.RefreshToken == refreshToken && u.Id == userId)
                .Select(u=>new UserRefreshTokenProjection(
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.LastLoginAt
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime> UpdateTokenAsync(UpdateTokenDTO dto)
        {
            var result = await _context.Database
                .SqlQueryRaw<RefreshTokenDTO>(
                    "SELECT * FROM update_refresh_token(@p0, @p1, @p2, @p3)",
                    dto.Id, dto.oldRefreshToken, dto.newRefreshToken, dto.newRefreshTokenExpiry
                )
                .FirstOrDefaultAsync();
            return result?.last_login_at ?? throw new Exception("Token update failed");
        }

        public async Task<UserVerificationProjection?> GetVerificationStatusByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email!)
                .Select(u => new UserVerificationProjection
                (
                    u.Id,
                    u.IsVerified
                ))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task MarkAsVerifiedAsync(int id)
        {
            await _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.IsVerified, true)
                );

        }

        public async Task<int> GetIdByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash)
        {
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s=>s
                    .SetProperty(u => u.PasswordHash, passwordHash)
                );
        }

        public async Task<int> LogoutAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.RefreshToken, (string?)null)
                    .SetProperty(u => u.RefreshTokenExpiry, (DateTime?)null)
                );
        }

        public async Task<LoginGetUserDTO?> GetAndUpdateLoginAsync(
        string email,
        string refreshToken,
        DateTime refreshTokenExpiry)
        {
            // 1. SQL sorgusunu oluştur
            var result = await _context.Database
                .SqlQueryRaw<LoginUserRawDTO>(
                    "SELECT * FROM public.get_and_update_user_login({0}, {1}, {2})",
                    email, refreshToken, refreshTokenExpiry)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (result == null)
                return null;

            return new LoginGetUserDTO(
            result.id,
            result.username,
            result.email,
            result.role,
            result.password_hash,
            result.last_login_at,
            result.refresh_token,
            result.refresh_token_expiry,
            result.is_verified,
            result.failed_login_attempts,
            result.lockout_end
            );
        }

        public async Task<PagedResult<CustomerDTO>> GetAllCustomersAsync(int page, int pageSize)
        {
            var query = _context.Users
                .Where(u => u.Role == UserRole.Customer)
                .OrderByDescending(u => u.CreatedDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var customers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new CustomerDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    Username = u.Username,
                    FullName = u.FullName,
                    IsVerified = u.IsVerified,
                    IsGuest = u.IsGuest,
                    Role = u.Role,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedDate,
                    TotalOrders = u.Orders.Count,
                    TotalReviews = u.Reviews.Count
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<CustomerDTO>
            {
                Data = customers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<UserProfileDTO?> GetUserProfileByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    Username = u.Username,
                    FullName = u.FullName,
                    IsVerified = u.IsVerified,
                    Role = u.Role,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedDate
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateUserProfileAsync(int userId, string username, string? fullName)
        {
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.Username, username)
                    .SetProperty(u => u.FullName, fullName)
                );
        }

        public async Task UpdateUserLockoutAsync(int userId, int failedAttempts, DateTime? lockoutEnd)
        {
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.FailedLoginAttempts, failedAttempts)
                    .SetProperty(u => u.LockoutEnd, lockoutEnd)
                );
        }
    }
}

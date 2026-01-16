using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.ReviewDTOs;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Repositories;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data.Repositories;
using PastirmaApi.Infrastructure.Email;
using Resend;
using System.Diagnostics;
using System.Security.Claims;

namespace PastirmaApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, IJwtService jwtService, IConfiguration configuration, IEmailService emailService, ILogger<UserService> logger)
        {
            _repository = repository;
            _jwtService = jwtService;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task RegisterUserAsync(RegisterUserDTO dto) {
            _logger.LogWarning("=== REGISTER USER START === Email: {Email}", dto.Email);

            if (await UserExistsAsync(dto.Email))
                throw new BusinessException("Bu email zaten kayıtlı");

            var user = new User
            {
                Username = dto.UserName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash, workFactor: 10),
                IsGuest = false,
                IsVerified = false,
                Role = UserRole.Customer
            };

            await _repository.AddAsync(user);
            _logger.LogWarning("User created in DB. ID: {UserId}", user.Id);

            // Create Email verification token
            var frontendUrl = _configuration["FrontendUrl"];
            _logger.LogWarning("FrontendUrl config: {FrontendUrl}", frontendUrl ?? "NOT CONFIGURED");

            var token = _jwtService.GenerateEmailVerificationToken(user.Email);
            var verifyLink = $"{frontendUrl}/account/verify-email?token={token}";
            _logger.LogWarning("Verify link: {VerifyLink}", verifyLink);

            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    EmailTemplateType.EmailVerification,
                    new Dictionary<string, string> {
                        { "VerifyLink", verifyLink },
                        { "Username", user.Username},
                        { "EmailTokenExpiresDays", _configuration["Jwt:EmailTokenExpiresDays"]!}
                    }
                );
            }
            catch (Exception ex)
            {
                // Log the error for debugging - email errors should not block registration
                _logger.LogError(ex, "Failed to send verification email to {Email}. User can request resend later.", user.Email);
            }

            _logger.LogWarning("=== REGISTER USER END ===");
        }

        public async Task VerifyEmailAsync(string token)
        {
            var email = _jwtService.ValidateEmailVerificationToken(token);
            if (string.IsNullOrEmpty(email))
                throw new BusinessException("Geçersiz veya süresi dolmuş token.");

            var user = await _repository.GetVerificationStatusByEmailAsync(email);
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            if (user.IsVerified)
                return; // zaten doğrulanmış
                        // 
            await _repository.MarkAsVerifiedAsync(user.Id);
        }

        public async Task ResendVerificationByTokenAsync(string expiredToken)
        {
            // Expired token’dan email bilgisini çıkart
            var principal = _jwtService.GetPrincipalFromExpiredToken(expiredToken);
            if (principal == null)
                throw new BusinessException("Geçersiz link");

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
                throw new BusinessException("Geçersiz token verisi");

            await ResendVerificationByEmailAsync(email);
        }

        public async Task ResendVerificationByEmailAsync(string email) {            
            
            var user = await _repository.GetByEmailAsync(email);
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            if (user.IsVerified)
                throw new BusinessException("Kullanıcı zaten doğrulanmış");
            
            var newToken = _jwtService.GenerateEmailVerificationToken(user.Email);
            var verifyLink = $"{_configuration["FrontendUrl"]}/account/verify-email?token={newToken}";
            
            await _emailService.SendEmailAsync(
                user.Email,
                EmailTemplateType.EmailVerification,
                new Dictionary<string, string> {
                    { "VerifyLink", verifyLink },
                    { "Username", user.Username!},
                    { "EmailTokenExpiresDays", _configuration["Jwt:EmailTokenExpiresDays"]!}
                }
            );
        }

        public async Task<LoginTransDTO> LoginUserAsync(LoginRequestDTO dto)
        {
            var totalStopwatch = Stopwatch.StartNew();

            // ===== 1. TOKEN'LARI ÖNCE OLUŞTUR (DB'ye gitmeden) =====
            var tokenStopwatch = Stopwatch.StartNew();
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpiresDays"]!)
            );
            tokenStopwatch.Stop();

            // ===== 2. TEK QUERY İLE USER GET + UPDATE =====
            var dbStopwatch = Stopwatch.StartNew();
            var user = await _repository.GetAndUpdateLoginAsync(
                dto.Email,
                refreshToken,
                refreshTokenExpiry
            );
            dbStopwatch.Stop();

            // ===== 3. BUSINESS LOGIC (C#'ta) =====
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            // ===== 3.1. ACCOUNT LOCKOUT CHECK (security: prevent brute force) =====
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                throw new BusinessException(
                    $"Hesap kilitli. {remainingMinutes} dakika sonra tekrar deneyin.",
                    StatusCodes.Status429TooManyRequests
                );
            }

            if (!user.IsVerified)
                throw new BusinessException(
                    "Email hesabı aktif edilmemiş.",
                    StatusCodes.Status504GatewayTimeout
                );

            // ===== 4. PASSWORD VERIFY =====
            var verifyStopwatch = Stopwatch.StartNew();
            if (!BCrypt.Net.BCrypt.Verify(dto.PasswordHash, user.PasswordHash))
            {
                // Increment failed login attempts
                var newFailedAttempts = user.FailedLoginAttempts + 1;

                // Lock account after 5 failed attempts (15 minutes)
                if (newFailedAttempts >= 5)
                {
                    var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    await _repository.UpdateUserLockoutAsync(user.Id, newFailedAttempts, lockoutEnd);
                    throw new BusinessException(
                        "Çok fazla başarısız giriş denemesi. Hesabınız 15 dakika kilitlendi.",
                        StatusCodes.Status429TooManyRequests
                    );
                }

                await _repository.UpdateUserLockoutAsync(user.Id, newFailedAttempts, null);
                throw new BusinessException($"Şifre yanlış. Kalan deneme hakkı: {5 - newFailedAttempts}");
            }
            verifyStopwatch.Stop();

            // ===== 4.1. RESET LOCKOUT ON SUCCESSFUL LOGIN =====
            if (user.FailedLoginAttempts > 0 || user.LockoutEnd.HasValue)
            {
                await _repository.UpdateUserLockoutAsync(user.Id, 0, null);
            }

            // ===== 5. ACCESS TOKEN OLUŞTUR =====
            var accessTokenStopwatch = Stopwatch.StartNew();
            var accessToken = _jwtService.GenerateAccessToken(
                new GenerateAccessTokenDTO(user.Id, user.Email,user.Username, user.Role.ToString())
            );
            accessTokenStopwatch.Stop();

            totalStopwatch.Stop();

            // ===== 6. LOGGING =====
            System.Diagnostics.Debug.WriteLine("=== HYBRID CTE LOGIN PERFORMANCE ===");
            System.Diagnostics.Debug.WriteLine($"  - Generate Tokens: {tokenStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - DB (GET + UPDATE): {dbStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - Verify Password: {verifyStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - Generate Access Token: {accessTokenStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - TOTAL: {totalStopwatch.ElapsedMilliseconds}ms");

            return new LoginTransDTO(
                user.Id,
                user.Username,
                user.Email,
                user.Role.ToString(),
                accessToken,
                refreshToken,
                refreshTokenExpiry,
                user.LastLoginAt
            );
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _repository.GetByEmailAsync(email);
            if (user == null)
            {
                // Email yoksa bile, bilgi sızdırmamak için sessizce çık
                return;
            }

            var token = _jwtService.GeneratePasswordResetToken(email);
            var resetLink = $"{_configuration["FrontendUrl"]}/account/reset-password?token={token}";

            await _emailService.SendEmailAsync(
                user.Email,
                EmailTemplateType.PasswordReset,
                new Dictionary<string, string> {
                    { "ResetLink", resetLink },
                    { "Username", user.Username!},
                    { "PasswordResetTokenExpiresMinutes", _configuration["Jwt:PasswordResetTokenExpiresMinutes"]!}
                }
            );
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var email = _jwtService.ValidatePasswordResetToken(dto.Token);
            if (email == null)            
                throw new BusinessException("Link geçersiz veya süresi dolmuş");
            

            var userId = await _repository.GetIdByEmailAsync(email);
            if (userId == 0) 
                throw new BusinessException("Kullanıcı bulunamadı");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repository.UpdatePasswordAsync(userId,passwordHash);
        }

        public async Task<LoginTransDTO> RefreshAccessTokenAsync(string oldRefreshToken, string accessToken)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var tokenReadStopwatch = Stopwatch.StartNew();
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                throw new BusinessException("Tekrar Giriş yapınız");
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId))
                throw new BusinessException("Tekrar Giriş yapınız");
            var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                throw new BusinessException("Tekrar Giriş yapınız");
            var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                throw new BusinessException("Tekrar Giriş yapınız");
            var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
                throw new BusinessException("Tekrar Giriş yapınız");
            tokenReadStopwatch.Stop();

            var tokenCreateStopwatch = Stopwatch.StartNew();
            var newAccessToken = _jwtService.GenerateAccessToken(new GenerateAccessTokenDTO(Convert.ToInt32(userId), userEmail, userName, userRole));
            var newRefreshToken = _jwtService.GenerateRefreshToken();            
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiresDays"]!));
            tokenCreateStopwatch.Stop();

            var dbStopwatch = Stopwatch.StartNew();
            var result= await _repository.UpdateTokenAsync(new UpdateTokenDTO(Convert.ToInt32(userId), oldRefreshToken, newRefreshToken, newRefreshTokenExpiry));
            dbStopwatch.Stop();
            totalStopwatch.Stop();
            // ===== 6. LOGGING =====
            System.Diagnostics.Debug.WriteLine("=== REFRESH TOKEN PERFORMANCE ===");
            System.Diagnostics.Debug.WriteLine($"  - Read AccessToken: {tokenReadStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - Create Tokens: {tokenCreateStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - DB (GET + UPDATE): {dbStopwatch.ElapsedMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"  - TOTAL: {totalStopwatch.ElapsedMilliseconds}ms");
            return new LoginTransDTO(Convert.ToInt32(userId), userName, userEmail, userRole, newAccessToken, newRefreshToken, newRefreshTokenExpiry, result);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _repository.EmailExistsAsync(email);
        }

        public async Task LogoutAsync(int userId)
        {
            var user = await _repository.LogoutAsync(userId);
            if (user == 0) throw new BusinessException("Kullanıcı bulunamadı");
        }

        public async Task<PagedResult<CustomerDTO>> GetAllCustomersAsync(int page, int pageSize)
        {
            return await _repository.GetAllCustomersAsync(page, pageSize);
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(int userId)
        {
            var profile = await _repository.GetUserProfileByIdAsync(userId);
            if (profile == null)
                throw new BusinessException("Kullanıcı bulunamadı");
            return profile;
        }

        public async Task<UserProfileDTO> UpdateUserProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            await _repository.UpdateUserProfileAsync(userId, dto.Username, dto.FullName);

            var updatedProfile = await _repository.GetUserProfileByIdAsync(userId);
            if (updatedProfile == null)
                throw new BusinessException("Profil güncellenemedi");

            return updatedProfile;
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO dto)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            if (user.PasswordHash == null)
                throw new BusinessException("Bu hesap için şifre değiştirme işlemi yapılamaz");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new BusinessException("Mevcut şifre yanlış");

            // Hash new password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 10);

            // Update password
            await _repository.UpdatePasswordAsync(userId, newPasswordHash);
        }
    }
}

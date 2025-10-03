using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Repositories;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data.Repositories;
using PastirmaApi.Infrastructure.Email;
using Resend;
using System.Security.Claims;

namespace PastirmaApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository repository, IJwtService jwtService, IConfiguration configuration, IEmailService emailService)
        {
            _repository = repository;
            _jwtService = jwtService;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task RegisterUserAsync(RegisterUserDTO dto) {

            if (await UserExistsAsync(dto.Email))
                throw new AuthException("Bu email zaten kayıtlı");

            var user = new User
            {
                Username = dto.UserName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
                IsGuest = false,
                IsVerified = false,
                Role = UserRole.Customer
            };

            await _repository.AddAsync(user);

            // Create Email verification token
            var token = _jwtService.GenerateEmailVerificationToken(user.Email);
            var verifyLink = $"{_configuration["FrontendUrl"]}/account/verify-email?token={token}";

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
                //Email errors should not go to the user. We will send a verification email again at the login   
            }
        }

        public async Task VerifyEmailAsync(string token)
        {
            var email = _jwtService.ValidateEmailVerificationToken(token);
            if (string.IsNullOrEmpty(email))
                throw new BusinessException("Geçersiz veya süresi dolmuş token.");

            var user = await _repository.GetByEmailAsync(email);
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            if (user.IsVerified)
                return; // zaten doğrulanmış

            user.IsVerified = true;
            await _repository.UpdateAsync(user);
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

        public async Task<LoginTransDTO> LoginUserAsync(LoginUserDTO dto)
        {
            var user = await _repository.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new BusinessException("Kullanıcı bulunamadı");

            if (!BCrypt.Net.BCrypt.Verify(dto.PasswordHash, user.PasswordHash))
                throw new BusinessException("Şifre yanlış");

            if (!user.IsVerified) //We gave HttpStatusCode 504 due to Email Service error. Frontend will act accordingly
                throw new BusinessException("Email hesabı aktif edilmemiş.",StatusCodes.Status504GatewayTimeout);

            user.LastLoginAt = DateTime.UtcNow;
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiresDays"]!));
            await _repository.UpdateAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(user);

            return new LoginTransDTO( user.Id, user.Username, user.Email, user.Role.ToString(), accessToken, refreshToken, user.RefreshTokenExpiry);
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
            

            var user = await _repository.GetByEmailAsync(email);
            if (user == null) 
                throw new BusinessException("Kullanıcı bulunamadı");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repository.UpdateAsync(user);
        }

        public async Task<LoginTransDTO> RefreshAccessTokenAsync(string refreshToken, string accessToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                throw new AuthException("Tekrar Giriş yapınız");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId))
                throw new AuthException("Tekrar Giriş yapınız");

            var user = await _repository.GetByRefreshTokenAsync(refreshToken);
            if (user == null)
                throw new AuthException("Tekrar Giriş yapınız");

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiresDays"]!));
            await _repository.UpdateAsync(user);

            return new LoginTransDTO(user.Id, user.Username, user.Email, user.Role.ToString(), newAccessToken, newRefreshToken, user.RefreshTokenExpiry);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _repository.EmailExistsAsync(email);
        }

        public async Task LogoutAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) throw new BusinessException("Kullanıcı bulunamadı");

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _repository.UpdateAsync(user);
        }
    }
}

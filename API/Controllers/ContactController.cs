using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.ContactDTOs;
using PastirmaApi.Application.DTOs.ReviewDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Infrastructure.Data;
using PastirmaApi.Infrastructure.Email;
using PastirmaApi.Infrastructure.GoogleCaptcha;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ICaptchaService _captchaService;

        public ContactController(
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<ContactController> logger,
            ApplicationDbContext context,
            ICaptchaService captchaService)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _captchaService = captchaService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormDTO dto)
        {
            try
            {
                // Verify Turnstile captcha token
                var captchaResult = await _captchaService.VerifyAsync(dto.CaptchaToken);

                if (!captchaResult.Success)
                {
                    _logger.LogWarning(
                        "Contact form captcha verification failed. Error codes: {ErrorCodes}",
                        string.Join(", ", captchaResult.ErrorCodes)
                    );

                    return BadRequest(new
                    {
                        message = "Captcha doğrulaması başarısız oldu. Lütfen tekrar deneyin."
                    });
                }

                // Save to database
                var submission = new ContactFormSubmission
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Subject = dto.Subject,
                    Message = dto.Message,
                    IsRead = false
                };

                _context.ContactFormSubmissions.Add(submission);
                await _context.SaveChangesAsync();

                // Get admin email from configuration
                var adminEmail = _configuration["Resend:AdminEmail"] ?? _configuration["Resend:EmailFrom"];

                if (string.IsNullOrEmpty(adminEmail))
                {
                    _logger.LogWarning("Admin email not configured, skipping email notification");
                }
                else
                {
                    // Prepare template values
                    var templateValues = new Dictionary<string, string>
                    {
                        { "Name", dto.Name },
                        { "Email", dto.Email },
                        { "Phone", dto.Phone ?? "Belirtilmemiş" },
                        { "Subject", dto.Subject },
                        { "Message", dto.Message },
                        { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") }
                    };

                    // Send email to admin
                    await _emailService.SendEmailAsync(
                        adminEmail,
                        EmailTemplateType.ContactForm,
                        templateValues
                    );
                }

                _logger.LogInformation(
                    "Contact form submitted from {Email} with subject: {Subject}",
                    dto.Email,
                    dto.Subject
                );

                return Ok(new { message = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form submission");
                return StatusCode(500, new { message = "Mesaj gönderilirken bir hata oluştu. Lütfen tekrar deneyin." });
            }
        }

        [HttpGet("messages")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<ContactMessageDTO>>> GetContactMessages(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.ContactFormSubmissions
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedDate);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var messages = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ContactMessageDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Subject = c.Subject,
                        Message = c.Message,
                        IsRead = c.IsRead,
                        ReadAt = c.ReadAt,
                        IsReplied = c.IsReplied,
                        RepliedAt = c.RepliedAt,
                        Notes = c.Notes,
                        CreatedDate = c.CreatedDate
                    })
                    .ToListAsync();

                return Ok(new PagedResult<ContactMessageDTO>
                {
                    Data = messages,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contact messages");
                return StatusCode(500, new { message = "Mesajlar yüklenirken bir hata oluştu." });
            }
        }

        [HttpPut("messages/{id}/read")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var message = await _context.ContactFormSubmissions
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (message == null)
                {
                    return NotFound(new { message = "Mesaj bulunamadı." });
                }

                if (!message.IsRead)
                {
                    message.IsRead = true;
                    message.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Mesaj okundu olarak işaretlendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
                return StatusCode(500, new { message = "Mesaj güncellenirken bir hata oluştu." });
            }
        }

        [HttpPost("messages/{id}/reply")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyToMessage(int id, [FromBody] ReplyMessageDTO dto)
        {
            try
            {
                var message = await _context.ContactFormSubmissions
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (message == null)
                {
                    return NotFound(new { message = "Mesaj bulunamadı." });
                }

                // Prepare reply email using a generic template
                var templateValues = new Dictionary<string, string>
                {
                    { "RecipientName", message.Name },
                    { "Subject", dto.Subject },
                    { "Message", dto.Message },
                    { "OriginalMessage", message.Message },
                    { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") }
                };

                // Send reply email
                try
                {
                    await _emailService.SendEmailAsync(
                        message.Email,
                        EmailTemplateType.ContactFormReply,
                        templateValues
                    );

                    // Mark as replied
                    message.IsReplied = true;
                    message.RepliedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Reply sent to {Email} for message {MessageId}",
                        message.Email,
                        id
                    );

                    return Ok(new { message = "Yanıt başarıyla gönderildi." });
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Email could not be sent, but reply was processed");
                    return Ok(new { message = "Yanıt kaydedildi ancak email gönderilemedi. Lütfen network ayarlarınızı kontrol edin." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reply to message {MessageId}", id);
                return StatusCode(500, new { message = "Yanıt gönderilirken bir hata oluştu. Lütfen tekrar deneyin." });
            }
        }
    }
}

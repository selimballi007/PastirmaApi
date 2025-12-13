using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using Resend;

namespace PastirmaApi.Infrastructure.Email
{
    public class EmailService : IEmailService
    {       
        private readonly IResend _resend;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailTemplateProvider _templateProvider;

        public EmailService(
            IConfiguration config,
            IResend resend,
            ILogger<EmailService> logger,
            IEmailTemplateProvider templateProvider)
        {
            _resend = resend;
            _config = config;
            _logger = logger;
            _templateProvider = templateProvider;
        }

        public async Task SendEmailAsync(string toEmail, EmailTemplateType templateType, IDictionary<string, string> values)
        {
            var message = new EmailMessage
            {
                From = _config["Resend:EmailFrom"]!,
                To = toEmail,
                Subject = _templateProvider.GetSubject(templateType),
                HtmlBody = _templateProvider.GetHtmlBody(templateType, values)
            };

            try
            {
                var result = await _resend.EmailSendAsync(message);
            }
            catch (ResendException ex)
            {
                _logger.LogError(ex, "Email gönderilemedi. - " + ex.ErrorType.ToString()+" - "+ ex.StatusCode.ToString());
                throw new EmailException(
                    "Email servisine ulaşılamıyor. Lütfen daha sonra tekrar deneyin.",
                    StatusCodes.Status502BadGateway
                );
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "TimeoutException: Email gönderimi zaman aşımına uğradı.");
                throw new EmailException(
                    "Email servisi yanıt vermedi. Lütfen tekrar deneyin.",
                    StatusCodes.Status504GatewayTimeout
                );
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "FormatException: Geçersiz email adresi.");
                throw new EmailException(
                    "Geçersiz email adresi.",
                    StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen email hatası.");
                throw new EmailException(
                    "Email gönderilemedi. Lütfen tekrar deneyin.",
                    StatusCodes.Status500InternalServerError
                );
            }
        }
    }
}

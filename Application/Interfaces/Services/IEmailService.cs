using PastirmaApi.Infrastructure.Email;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, EmailTemplateType templateType, IDictionary<string, string> values);
    }
}

using PastirmaApi.Infrastructure.Email;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IEmailTemplateProvider
    {
        string GetSubject(EmailTemplateType type);
        string GetHtmlBody(EmailTemplateType type, IDictionary<string, string> values);
        string GetBodyFileName(EmailTemplateType type);
    }
}

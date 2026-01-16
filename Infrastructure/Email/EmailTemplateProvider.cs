using PastirmaApi.Application.Interfaces.Services;

namespace PastirmaApi.Infrastructure.Email
{
    public class EmailTemplateProvider : IEmailTemplateProvider
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public EmailTemplateProvider(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public string GetSubject(EmailTemplateType type)
        {
            return type switch
            {
                EmailTemplateType.PasswordReset => "Şifre Sıfırlama - Pastırma",
                EmailTemplateType.EmailVerification => "Email Doğrulama - Pastırma",
                EmailTemplateType.Welcome => $"Hoşgeldiniz - Pastırma",
                EmailTemplateType.ContactForm => "Yeni İletişim Formu Mesajı - Pastırma Adası",
                EmailTemplateType.ContactFormReply => "Mesajınıza Yanıt - Pastırma Adası",
                _ => "Pastırma"
            };
        }

        public string GetBodyFileName(EmailTemplateType type)
        {
            return type switch
            {
                EmailTemplateType.PasswordReset => "ResetPasswordTemplate.html",
                EmailTemplateType.EmailVerification => "EmailVerificationTemplate.html",
                EmailTemplateType.Welcome => $"WelcomeTemplate.html",
                EmailTemplateType.ContactForm => "ContactFormTemplate.html",
                EmailTemplateType.ContactFormReply => "ContactFormReplyTemplate.html",
                _ => "Pastırma"
            };
        }

        public string GetHtmlBody(EmailTemplateType type, IDictionary<string, string> values)
        {
            var path = Path.Combine(_env.ContentRootPath, "Infrastructure", "Email", "Templates", GetBodyFileName(type));
            try
            {
                var template = File.ReadAllText(path);
                foreach (var kv in values)
                {
                    template = template.Replace($"{{{{{kv.Key}}}}}", kv.Value);
                }

                return template;
            }
            catch (Exception er)
            {

                throw new Exception(er.Message);
            }
        }
    }

    public enum EmailTemplateType
    {
        PasswordReset,
        EmailVerification,
        Welcome,
        ContactForm,
        ContactFormReply
    }

}

using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Mistria.Domain.Models;
using Mistria.Domain.Services;
using MailKit.Security;

namespace Mistria.Application
{
    public class MailService : IMailService
    {
        private readonly MailSettings _options;

        public MailService(IOptions<MailSettings> options)
        {
            _options = options.Value;
        }

        public async Task SendEmailAsync(Email email)
        {
            var mail = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_options.Email),
                Subject = email.Subject ?? "New Contact Form Submission"
            };
            mail.To.Add(MailboxAddress.Parse(email.To));
            var builder = new BodyBuilder();

            builder.TextBody = email.Body;
            mail.Body = builder.ToMessageBody();
            mail.From.Add(new MailboxAddress(_options.DisplayName, _options.Email));

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_options.Email, _options.Password);
            await smtp.SendAsync(mail);
            await smtp.DisconnectAsync(true);
        }
    }
}

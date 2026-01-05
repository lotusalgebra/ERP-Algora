using Algora.Erp.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Algora.Erp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailMessage message)
    {
        var email = CreateMimeMessage(message);
        await SendAsync(email);
    }

    public async Task SendEmailWithAttachmentAsync(EmailMessage message, byte[] attachment, string attachmentName, string contentType)
    {
        var email = CreateMimeMessage(message);

        // Add attachment
        var builder = new BodyBuilder();

        if (message.IsHtml)
        {
            builder.HtmlBody = message.Body;
        }
        else
        {
            builder.TextBody = message.Body;
        }

        builder.Attachments.Add(attachmentName, attachment, ContentType.Parse(contentType));
        email.Body = builder.ToMessageBody();

        await SendAsync(email);
    }

    private MimeMessage CreateMimeMessage(EmailMessage message)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(message.To));

        if (!string.IsNullOrEmpty(message.Cc))
        {
            email.Cc.Add(MailboxAddress.Parse(message.Cc));
        }

        if (!string.IsNullOrEmpty(message.Bcc))
        {
            email.Bcc.Add(MailboxAddress.Parse(message.Bcc));
        }

        email.Subject = message.Subject;

        if (message.IsHtml)
        {
            email.Body = new TextPart("html") { Text = message.Body };
        }
        else
        {
            email.Body = new TextPart("plain") { Text = message.Body };
        }

        return email;
    }

    private async Task SendAsync(MimeMessage email)
    {
        using var smtp = new SmtpClient();

        try
        {
            _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", _settings.SmtpServer, _settings.SmtpPort);

            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, secureSocketOptions);

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                await smtp.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword);
            }

            await smtp.SendAsync(email);

            _logger.LogInformation("Email sent successfully to {To}", email.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", email.To);
            throw;
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}

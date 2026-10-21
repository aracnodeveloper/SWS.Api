using App.Shared.Domain;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace App.Shared.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}

public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.User) || string.IsNullOrWhiteSpace(opts.Pass))
            throw new InvalidOperationException("SMTP no configurado (SMTP_USER / SMTP_PASS).");

        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Destinatario vacío.", nameof(to));

        var fromAddr = string.IsNullOrWhiteSpace(opts.From) ? opts.User : opts.From;
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(opts.FromName, fromAddr));
        message.To.Add(MailboxAddress.Parse(to.Trim()));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(opts.Host, opts.Port, opts.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
        await client.AuthenticateAsync(opts.User, opts.Pass.Replace(" ", ""), ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}

using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UserService.WebApi.Services.Realisations;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var settings = _config.GetSection("EmailSettings");

        var smtpClient = new SmtpClient(settings["SmtpServer"])
        {
            Port = int.Parse(settings["SmtpPort"]),
            Credentials = new NetworkCredential(settings["SenderEmail"], settings["SenderPassword"]),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(settings["SenderEmail"], settings["SenderName"]),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(email);

        await smtpClient.SendMailAsync(message);
    }
}
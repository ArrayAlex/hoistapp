using SendGrid;
using SendGrid.Helpers.Mail;

namespace hoistmt.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_configuration["SendGrid:FromEmail"], "Hoist"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        var response = await client.SendEmailAsync(msg);
    }
}

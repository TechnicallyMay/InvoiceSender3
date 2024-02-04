using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace InvoiceSender3.Email;

public class MailGridEmailSender : IEmailSender
{
    private readonly ILogger<MailGridEmailSender> _logger;
    private readonly MailGridOptions _options;

    public MailGridEmailSender(IOptions<MailGridOptions> optionsAccessor, ILogger<MailGridEmailSender> logger)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
    }


    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(_options.SendGridKey))
        {
            throw new Exception("Null SendGridKey");
        }
        
        await Execute(_options.SendGridKey, subject, message, toEmail);
    }

    private async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        _logger.LogInformation("Sending email to {Email}", toEmail);
        
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("masonwells01@gmail.com", "Invoice Sender"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking. See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        
        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Successfully sent email.");
            return;
        }

        _logger.LogError("There was an error sending email: {Error}", response.Body);
    }
}
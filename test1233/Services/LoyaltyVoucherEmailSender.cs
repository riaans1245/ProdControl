using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using test1233.Models;

namespace test1233.Services;

public class LoyaltyVoucherEmailSender(IConfiguration configuration, ILogger<LoyaltyVoucherEmailSender> logger) : ILoyaltyVoucherEmailSender
{
    public bool TrySend(AppUser user, string voucherCode)
    {
        var host = configuration["Smtp:Host"];
        var from = configuration["Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            logger.LogWarning("Loyalty voucher {VoucherCode} was issued but SMTP is not configured.", voucherCode);
            return false;
        }

        try
        {
            using var client = new SmtpClient(host, configuration.GetValue("Smtp:Port", 587))
            {
                EnableSsl = configuration.GetValue("Smtp:EnableSsl", true),
                Credentials = new NetworkCredential(configuration["Smtp:Username"], configuration["Smtp:Password"])
            };
            using var message = new MailMessage(from, user.EmailAddress)
            {
                Subject = "Your $20 Easy Eats loyalty voucher",
                Body = $"Congratulations {user.Name}! You have earned a $20 voucher. Your voucher code is {voucherCode}."
            };
            client.Send(message);
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not email loyalty voucher {VoucherCode}.", voucherCode);
            return false;
        }
    }
}

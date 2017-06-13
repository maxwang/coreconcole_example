using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataImporter.Framework.Services
{
    public class SMSEmailSender : IEmailSender
    {

        public async Task SendEmailAsync(string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("System", "dataimporter@smsetechnologies.com"));
            emailMessage.To.Add(new MailboxAddress("", "support1080agile@1080agile.com"));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                var credentials = new NetworkCredential
                {
                    UserName = "sms_mail", // replace with valid value
                    Password = "smsMAIL01" // replace with valid value
                };
                client.LocalDomain = "smsetechnologies.com";
                await client.ConnectAsync("192.168.29.76", 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.AuthenticateAsync(credentials);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
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
        private SMTPOptions _smtpSettings;

        public SMSEmailSender(IOptions<SMTPOptions> options)
        {
            _smtpSettings = options.Value;
        }

        public async Task SendEmailAsync(string subject, string message)
        {
            var emailMessage = new MimeMessage();
            
            emailMessage.From.Add(new MailboxAddress(_smtpSettings.From, _smtpSettings.FromAddress));
            emailMessage.To.Add(new MailboxAddress(_smtpSettings.ToAddress));


            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                var credentials = new NetworkCredential
                {
                    UserName = _smtpSettings.Username, 
                    Password = _smtpSettings.Password 
                };
                
                client.LocalDomain = _smtpSettings.LocalDomain;
                await client.ConnectAsync(_smtpSettings.SMTPSeverIP, _smtpSettings.SMTPPort, SecureSocketOptions.None).ConfigureAwait(false);
                await client.AuthenticateAsync(credentials);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}

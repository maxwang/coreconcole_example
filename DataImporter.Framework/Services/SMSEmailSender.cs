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
        private string _from;
        private string _fromAddress;
        private string _toAddress;
        private string _username;
        private string _password;
        private string _localDomain;
        private string _smtpServerIp;

        public SMSEmailSender(IOptions<SMTPOptions> options)
        {
            _username = options.Value.Username;
            _password = options.Value.Password;
            _localDomain = options.Value.LocalDomain;
            _smtpServerIp = options.Value.SMTPSeverIP;
            _from = options.Value.From;
            _fromAddress = options.Value.FromAddress;
            _toAddress = options.Value.ToAddress;
        }

        public async Task SendEmailAsync(string subject, string message)
        {
            var emailMessage = new MimeMessage();

            //emailMessage.From.Add(new MailboxAddress("Zoho Data Importer", "dataimporter@smsetechnologies.com"));
            //emailMessage.To.Add(new MailboxAddress("mwang@1080agile.com"));

            emailMessage.From.Add(new MailboxAddress(_from, _fromAddress));
            emailMessage.To.Add(new MailboxAddress(_toAddress));


            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                var credentials = new NetworkCredential
                {
                    //UserName = "sms_mail", // replace with valid value
                    //Password = "smsMAIL01" // replace with valid value
                    UserName = _username, // replace with valid value
                    Password = _password // replace with valid value
                };
                //client.LocalDomain = "smsetechnologies.com";
                //await client.ConnectAsync("192.168.29.76", 25, SecureSocketOptions.None).ConfigureAwait(false);
                client.LocalDomain = _localDomain;
                await client.ConnectAsync(_smtpServerIp, 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.AuthenticateAsync(credentials);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}

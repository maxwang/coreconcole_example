using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SMTPTest
{
    class Program
    {
        static void Main(string[] args)
        {


            //var emailMessage = new MimeMessage();

            //emailMessage.From.Add(new MailboxAddress("System", "dataimporter@smsetechnologies.com"));
            //emailMessage.To.Add(new MailboxAddress("Max", "mwang@1080agile.com"));
            //emailMessage.Subject = "this is a test";
            //emailMessage.Body = new TextPart("plain") { Text = "test message from core" };

            //using (var client = new SmtpClient())
            //{
            //    var credentials = new NetworkCredential
            //    {
            //        UserName = "sms_mail", // replace with valid value
            //        Password = "smsMAIL01" // replace with valid value
            //    };
            //    client.LocalDomain = "smsetechnologies.com";
            //    client.Connect("192.168.29.76", 25, SecureSocketOptions.None);
            //    client.Authenticate(credentials);
            //    client.Send(emailMessage);
            //    client.Disconnect(true);
            //}

            Task.Factory.StartNew(async () =>
            {
                await SendEmailAsync("test", "this is a test");
            });
            
            Console.ReadLine();

        }

        public async static Task SendEmailAsync(string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Zoho Data Importer", "dataimporter@smsetechnologies.com"));
            emailMessage.To.Add(new MailboxAddress("mwang@1080agile.com"));
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
                await client.AuthenticateAsync(credentials);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

    }
}
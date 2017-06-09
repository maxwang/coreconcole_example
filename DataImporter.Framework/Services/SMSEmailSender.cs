using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataImporter.Framework.Services
{
    public class SMSEmailSender : IEmailSender
    {
        public void Dispose()
        {
            
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            Console.WriteLine("Send Email");
            await Task.FromResult(0);
        }
    }
}

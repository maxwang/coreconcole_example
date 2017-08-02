using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.Framework.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string subject, string message);
        Task SendEmailAsync(string subject, string message, IList<string> toList);
    }
}

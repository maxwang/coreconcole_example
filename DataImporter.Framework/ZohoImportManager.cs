using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.Framework
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class ZohoImportManager
    {
        protected readonly IZohoCRMDataRepository _zohoRepository;
        protected readonly IEmailSender _emailSender;

        public event EventHandler<MessageEventArgs> DisplayMessage;

        protected virtual void OnDisplayMessage(MessageEventArgs e)
        {
            if (DisplayMessage != null)
            {
                DisplayMessage(this, e);
            }
        }

        public ZohoImportManager(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            _zohoRepository = zohoRepository;
            _emailSender = emailSender;
        }

        public async Task StartImportAsync()
        {
            await Task.FromResult(true);
            
        }


        public async Task StopImportAsync()
        {
            await Task.FromResult(true);
        }



    }
}

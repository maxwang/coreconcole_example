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
        private bool _stopFlag = false;

        public event EventHandler<MessageEventArgs> DisplayMessage;

        private void OnDisplayMessage(MessageEventArgs e)
        {
            DisplayMessage?.Invoke(this, e);
        }

        public ZohoImportManager(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            _zohoRepository = zohoRepository;
            _emailSender = emailSender;
        }

        public async Task StartImportAsync()
        {
            OnDisplayMessage(new MessageEventArgs { Message = "StartImportAsync" });

            int i = 0;

            while (_stopFlag == false)
            {
                OnDisplayMessage(new MessageEventArgs { Message = string.Format("StartImportAsync {0}", i) });
                System.Threading.Thread.Sleep(10000);
            }

            await Task.FromResult(true);

            OnDisplayMessage(new MessageEventArgs { Message = "StartImportAsync STOPPED" });

        }


        public async Task StopImportAsync()
        {
            _stopFlag = true;

            OnDisplayMessage(new MessageEventArgs { Message = "StopImportAsync" });
            await Task.FromResult(true);
        }



    }
}

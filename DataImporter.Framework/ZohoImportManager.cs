using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataImporter.Framework
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class ZohoImportManager: IDisposable
    {
        protected readonly IZohoCRMDataRepository _zohoRepository;
        protected readonly IEmailSender _emailSender;
        

        private CancellationTokenSource _cts;
        private CancellationToken _token;

        public event EventHandler<MessageEventArgs> DisplayMessage;

        private void OnDisplayMessage(MessageEventArgs e)
        {
            DisplayMessage?.Invoke(this, e);
        }

        public ZohoImportManager(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            _zohoRepository = zohoRepository;
            _emailSender = emailSender;
            _cts = new CancellationTokenSource();
            _token = _cts.Token;
        }

        public async Task StartImportAsync()
        {
            PartnerPortalImporter importer = new PartnerPortalImporter(_zohoRepository, _emailSender);
            await importer.ImportDataAsync(_token);
        }


        public async Task StopImportAsync()
        {
            
            OnDisplayMessage(new MessageEventArgs { Message = "StopImportAsync" });

            if(_cts != null)
            {
                _cts.Cancel();
            }
            await Task.FromResult(true);
        }

        //private async Task StartParterPortalImporter()
        //{
        //    _token.ThrowIfCancellationRequested();

        //    for(int i =0; i< 10; i++)
        //    {
        //        _token.ThrowIfCancellationRequested();
        //        OnDisplayMessage(new MessageEventArgs { Message = i.ToString() });
        //        System.Threading.Thread.Sleep(5000);
        //    }

        //    await Task.FromResult(true);
        //}

        public void Dispose()
        {
            if(_cts != null)
            {   
                _cts.Dispose();
            }
        }
    }
}

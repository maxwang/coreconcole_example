using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataImporter.Framework
{
    public abstract class ZohoImportBase
    {
        protected readonly IZohoCRMDataRepository _zohoRepository;
        protected readonly IEmailSender _emailSender;

        protected string TableName;

        public ZohoImportBase(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            _zohoRepository = zohoRepository;
            _emailSender = emailSender;
        }


        protected virtual async Task<string> GetNextUpdatedRecordAsync(string id = "")
        {
            return await Task.FromResult(string.Empty);
        }
        protected virtual async Task<bool> ProcessImport(string id)
        {
            return await Task.FromResult(false);
        }

        protected virtual async Task UpdateStatus(string id)
        {
            await Task.FromResult("");
        }
        

        public async Task ImportDataAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                var id = await GetNextUpdatedRecordAsync();
                
                while (!string.IsNullOrEmpty(id))
                {
                    ct.ThrowIfCancellationRequested();

                    var importResult = await ProcessImport(id);
                    if(importResult)
                    {
                        await UpdateStatus(id);
                    }
                    
                    ct.ThrowIfCancellationRequested();

                    id = await GetNextUpdatedRecordAsync(id);
                }

            }
            catch (Exception ex)
            {
                string subject = string.Format("{0} import error", TableName);
                await _emailSender.SendEmailAsync(subject, ex.StackTrace);
            }
        }
        
    }
}

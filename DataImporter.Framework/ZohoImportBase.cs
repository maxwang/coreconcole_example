using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.Framework
{
    public abstract class ZohoImportBase
    {
        protected readonly IZohoCRMDataRepository _zohoRepository;
        protected readonly IEmailSender _emailSender;

        protected string TableName;
        protected bool Stop;

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

        public void StopUpdate()
        {
            Stop = true;
        }



        public ZohoImportBase(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            _zohoRepository = zohoRepository;
            _emailSender = emailSender;
            Stop = false;
        }
        
        //public async Task ImportAysnc()
        //{
        //    while(true)
        //    {
        //        await ImportDataAsync();
        //        if(Stop)
        //        {
        //            break;
        //        }

        //        //wait for one minute to check
        //        System.Threading.Thread.Sleep(1000);

        //    }
        //}

        public async Task ImportDataAsync()
        {
            try
            {
                var id = await GetNextUpdatedRecordAsync();
                while(!string.IsNullOrEmpty(id))
                {
                    if(Stop)
                    {
                        break;
                    }

                    var importResult = await ProcessImport(id);
                    if(importResult)
                    {
                        await UpdateStatus(id);
                    }

                    id = await GetNextUpdatedRecordAsync(id);
                }

            }
            catch (Exception ex)
            {
                string subject = string.Format("{0} import error", TableName);
                await _emailSender.SendEmailAsync(subject, ex.StackTrace);
            }
        }


        public void test()
        {
            Console.WriteLine("test");
        }
        
    }
}

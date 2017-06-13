using DataImporter.Framework.Repository;
using DataImporter.Framework.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using DataImporter.Framework.Models;

namespace DataImporter.Framework
{
    public abstract class ZohoImportBase
    {
        protected readonly IZohoCRMDataRepository ZohoRepository;
        protected readonly IEmailSender EmailSender;
        protected string DefaultRoleName;

        protected string TableName;
        protected string PortalAction;

        public ZohoImportBase(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            ZohoRepository = zohoRepository;
            EmailSender = emailSender;
            DefaultRoleName = "External Admin";
        }


        protected virtual async Task<ZohoTableStatus> GetNextUpdatedRecordAsync(string id = "")
        {
            //var query = from ts in _zohoRepository.TableStatus
            //            where ts.TableName.Equals(this.TableName, StringComparison.CurrentCultureIgnoreCase)
            //            && ts.LastActionTime > ts.PortalActionTime
            //            orderby ts.LastActionTime
            //            select ts;

            var result = ZohoRepository.TableStatus
                        .Where(x => x.TableName.Equals(TableName, StringComparison.CurrentCultureIgnoreCase))
                        .Where(x => x.PortalActionTime == null || x.LastActionTime > x.PortalActionTime);
            
            if(!string.IsNullOrEmpty(id))
            {
                result = result.Where(x => x.RecordID.CompareTo(id) > 0);
            }

            var results = result.OrderBy(x => x.LastActionTime).ToAsyncEnumerable();
            
            return await results.Select(x => x).SingleOrDefault();
        }
        protected virtual async Task<PortalActionResult> ProcessImport(string id)
        {
            return await Task.FromResult(new PortalActionResult { IsSuccess = false });
        }

        protected virtual async Task UpdateRecordPortalActionResultAsync(ZohoTableStatus status)
        {
            await ZohoRepository.UpdateTableStatusAsync(status);
        }
        

        public async Task ImportDataAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                var recordStatus = await GetNextUpdatedRecordAsync();
                
                while (recordStatus != null)
                {
                    ct.ThrowIfCancellationRequested();

                    var importResult = await ProcessImport(recordStatus.RecordID);

                    recordStatus.PortalAction = PortalAction;
                    recordStatus.PortalActionResult = importResult.Resutl;
                    await UpdateRecordPortalActionResultAsync(recordStatus);

                    if(importResult.IsSuccess == false)
                    {
                        await EmailSender.SendEmailAsync(string.Format("Error:{0}-{1}", TableName, PortalAction), importResult.Resutl);
                    }

                    ct.ThrowIfCancellationRequested();

                    var id = await GetNextUpdatedRecordAsync();
                }

            }
            catch (Exception ex)
            {
                string subject = string.Format("{0} import error", TableName);
                await EmailSender.SendEmailAsync(subject, ex.StackTrace);
            }
        }
        
    }
}

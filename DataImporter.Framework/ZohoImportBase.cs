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

        private string _loggerName;

        public event EventHandler<MessageEventArgs> DisplayMessage;

        private void OnDisplayMessage(string message)
        {
            DisplayMessage?.Invoke(this, new MessageEventArgs { Message = message });
        }

        public ZohoImportBase(IZohoCRMDataRepository zohoRepository, IEmailSender emailSender)
        {
            ZohoRepository = zohoRepository;
            EmailSender = emailSender;
            DefaultRoleName = "ExternalAdmin";
            _loggerName = "importer";
        }


        protected virtual ZohoTableStatus GetNextUpdatedRecord(string id = "")
        {
            //var query = from ts in _zohoRepository.TableStatus
            //            where ts.TableName.Equals(this.TableName, StringComparison.CurrentCultureIgnoreCase)
            //            && ts.LastActionTime > ts.PortalActionTime
            //            orderby ts.LastActionTime
            //            select ts;

            var result = ZohoRepository.TableStatus
                        .Where(x => x.TableName.Equals(TableName, StringComparison.CurrentCultureIgnoreCase))
                        .Where(x => x.PortalActionTime == null || x.LastActionTime > x.PortalActionTime)
                        .Where( x => string.IsNullOrEmpty(x.PortalAction) || !x.PortalAction.StartsWith("[Start]"));
            
            if(!string.IsNullOrEmpty(id))
            {
                result = result.Where(x => x.RecordID.CompareTo(id) > 0);
            }

            var results = result.OrderBy(x => x.LastActionTime);
            
            return results.FirstOrDefault();
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

                OnDisplayMessage(string.Format("{0}: Start importing.....", TableName));

                var recordStatus = GetNextUpdatedRecord();
                
                while (recordStatus != null)
                {
                    ct.ThrowIfCancellationRequested();

                    //update portal status
                    recordStatus.PortalAction = string.Format("[Start]:{0}", PortalAction);
                    recordStatus.PortalActionResult = string.Empty;
                    await UpdateRecordPortalActionResultAsync(recordStatus);

                    await ZohoRepository.AddActionLogAsync(new ZohoActionLog
                    {
                        TableName = TableName,
                        Action = string.Format("[Start]:{0}", PortalAction),
                        ActionData = recordStatus.RecordID,
                        ActionResult = string.Empty,
                        CreatedBy = _loggerName,
                        CreatedTime = DateTime.Now,
                        Stageindicator = 1
                    });

                    OnDisplayMessage(string.Format("{0}: Start import {1}", TableName, recordStatus.RecordID));

                    var importResult = await ProcessImport(recordStatus.RecordID);
                    

                    recordStatus.PortalAction = PortalAction;
                    recordStatus.PortalActionResult = importResult.Result;
                    await UpdateRecordPortalActionResultAsync(recordStatus);

                    await ZohoRepository.AddActionLogAsync(new ZohoActionLog
                    {
                        TableName = TableName,
                        Action = string.Format("[{0}]:{1}", importResult.IsSuccess == true ? "Finished" : "Error", PortalAction),
                        ActionData = recordStatus.RecordID,
                        ActionResult = importResult.Result,
                        CreatedBy = _loggerName,
                        CreatedTime = DateTime.Now,
                        Stageindicator = 2
                    });

                    OnDisplayMessage(string.Format("{0}: import {1}: {2}", TableName, recordStatus.RecordID, importResult.IsSuccess == true ? "Finished" : "Error"));

                    if (importResult.IsSuccess == false)
                    {
                        await EmailSender.SendEmailAsync(string.Format("Error:{0}-{1}", TableName, PortalAction), importResult.Result);
                    }

                    ct.ThrowIfCancellationRequested();

                    recordStatus = GetNextUpdatedRecord(recordStatus.RecordID);
                }

                OnDisplayMessage(string.Format("{0}: Finished!", TableName));

            }
            catch (Exception ex)
            {
                string subject = string.Format("{0} import error", TableName);
                await EmailSender.SendEmailAsync(subject, ex.StackTrace);

                //await ZohoRepository.AddActionLogAsync(new ZohoActionLog
                //{
                //    TableName = TableName,
                //    Action = "Exception",
                //    ActionData = ex.StackTrace,
                //    ActionResult = ex.Message,
                //    CreatedBy = _loggerName,
                //    CreatedTime = DateTime.Now,
                //    Stageindicator = 3
                //});
            }
        }
        
    }
}

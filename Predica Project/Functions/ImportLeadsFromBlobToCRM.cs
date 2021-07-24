using System;
using System.IO;
using AzureFunctions.Autofac;
using CrmSynchronizationApp.Handlers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Predica.Configs;

namespace PredicaProject
{
    [StorageAccount("BlobConnectionString")]
    //[DependencyInjectionConfig(typeof(AutofacConfig))]
    public static class ImportLeadsFromBlobToCRM
    {

        [FunctionName("ImportLeadsFromBlobToCRM")]
        public static void Run([BlobTrigger("leads/{name}")] Stream myBlob, string name, ILogger log)
        {
            if(myBlob == null)
            {
                log.LogInformation("Blob is null. No action will be executed.");
                return;
            }

            log.LogInformation($"File named: {name} was detected and will be processed.");

            try
            {
                ImportLeadHandler handler = new ImportLeadHandler(log);
                handler.ImportLeads(myBlob);
            }
            catch (Exception ex)
            {
                log.LogError($"Error occured during importing leads from blob storage to Dynamics 365. Error: {ex}");
            }
        }
    }
}

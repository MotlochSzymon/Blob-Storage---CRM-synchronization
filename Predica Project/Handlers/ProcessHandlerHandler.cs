using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Xrm.Tooling.Connector;
using AutoMapper;
using Predica.Configs;
using Microsoft.Extensions.Logging;

namespace CrmSynchronizationApp.Handlers
{
    public class ProcessHandlerHandler
    {
        protected IOrganizationService Service { get; set; }
        protected IMapper Mapper { get; set; }
        protected ILogger Log { get; set; }

        public ProcessHandlerHandler(ILogger Log)
        {
            ConnectToCrm();
            this.Mapper = MapInitializer.Activate();
            this.Log = Log;
        }

        public void ConnectToCrm()
        {
            Service = GetServiceClient();
        }

        public static IOrganizationService GetServiceClient()
        {
            var ServiceUrl = Environment.GetEnvironmentVariable("CRMBASEURL");
            var ClientId = Environment.GetEnvironmentVariable("CLIENTID");
            var ClientSecret = Environment.GetEnvironmentVariable("CLIENTSECRET");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            return new CrmServiceClient(new Uri(ServiceUrl), ClientId, ClientSecret, true, null);
        }
    }
}

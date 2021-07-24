using Autofac;
using AzureFunctions.Autofac.Configuration;
using Predica.Xrm.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predica.Configs
{
    public class AutofacConfig
    {
        public AutofacConfig()
        {
            DependencyInjection.Initialize(builder =>
            {
                builder.RegisterType<LeadRepository>().As<ILeadRepository>();
            });
        }
    }
}

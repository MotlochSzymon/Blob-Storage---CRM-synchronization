using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predica.Xrm.Core.DataAccess
{
    public interface ILeadRepository : IBaseRepository<Lead>
    {
        bool CheckIfLeadFromConferenceCanBeAdded(Lead lead);
        List<Lead> GetLeadByEmail(string email);
        QualifyLeadResponse QualifyLeadToContact(Guid leadId, bool shouldContactBeCreated);
    }
}

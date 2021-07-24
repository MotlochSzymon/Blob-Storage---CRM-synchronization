using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predica.Xrm.Core.DataAccess
{
    public class LeadRepository : BaseRepository<Lead>, ILeadRepository
    {
        public LeadRepository(IOrganizationService service) : base(service)
        {

        }

        public bool CheckIfLeadFromConferenceCanBeAdded(Lead lead)
        {
            if (lead == null)
            {
                throw new NullReferenceException("Lead is null!");
            }

            string email = lead.EMailAddress1;
            DateTime? conferenceBegin = lead.new_conferencebegindate;
            DateTime? conferenceEnd = lead.new_conferenceenddate;

            if (email == null || conferenceBegin == null || conferenceEnd == null)
            {
                return true;
            }

            using (var ctx = new OrganizationServiceContext(this.service))
            {
                return ctx.CreateQuery<Lead>()
                    .Where(x => x.EMailAddress1 == email && x.new_conferenceenddate >= conferenceBegin && x.new_conferencebegindate <= conferenceEnd)
                    .ToList()
                    .Any();
            }
        }

        public List<Lead> GetLeadByEmail(string email)
        {
            using (var ctx = new OrganizationServiceContext(this.service))
            {
                return ctx.CreateQuery<Lead>()
                    .Where(x => x.EMailAddress1 == email)
                    .ToList();
            }
        }

        public QualifyLeadResponse QualifyLeadToContact(Guid leadId, bool shouldContactBeCreated)
        {
            var qualifyLeadRequest = new QualifyLeadRequest
            {
                CreateContact = shouldContactBeCreated,
                LeadId = new EntityReference(Lead.EntityLogicalName, leadId),
                Status = new OptionSetValue((int)lead_statuscode.Qualified)
            };

            using (var ctx = new OrganizationServiceContext(this.service))
            {
                return (QualifyLeadResponse)ctx.Execute(qualifyLeadRequest);
            }
        }
    }
}

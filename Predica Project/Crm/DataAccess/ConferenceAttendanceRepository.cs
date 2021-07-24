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
    public class ConferenceAttendanceRepository : BaseRepository<new_conferenceattendance>, IConferenceAttendanceRepository
    {
        public ConferenceAttendanceRepository(IOrganizationService service) : base(service)
        {

        }

       public bool HadContactAnyConferenceAtThatTime(Guid? contactId, DateTime? conferenceBegin, DateTime? conferenceEnd)
        {
            if(!contactId.HasValue || conferenceBegin == null || conferenceEnd == null)
            {
                return false;
            }

            using (var ctx = new OrganizationServiceContext(this.service))
            {            
                return ctx.CreateQuery<new_conferenceattendance>()
                    .Where(x => x.new_contactid != null &&  x.new_contactid.Id == contactId.Value && 
                    x.new_conferenceleavedate >= conferenceBegin && x.new_conferencebegindate <= conferenceEnd)
                    .ToList()
                    .Any();
            }
        }


        //public bool CheckIfLeadFromConferenceCanBeAdded(Lead lead)
        //{
        //    if (lead == null)
        //    {
        //        throw new NullReferenceException("Lead is null!");
        //    }

        //    string email = lead.EMailAddress1;
        //    DateTime? conferenceBegin = lead.new_conferencebegindate;
        //    DateTime? conferenceEnd = lead.new_conferenceenddate;

        //    if (email == null || conferenceBegin == null || conferenceEnd == null)
        //    {
        //        return true;
        //    }

        //    using (var ctx = new OrganizationServiceContext(this.service))
        //    {
        //        return ctx.CreateQuery<Lead>()
        //            .Where(x => x.EMailAddress1 == email && x.new_conferenceenddate >= conferenceBegin && x.new_conferencebegindate <= conferenceEnd)
        //            .ToList()
        //            .Any();
        //    }
        //}


    }
}

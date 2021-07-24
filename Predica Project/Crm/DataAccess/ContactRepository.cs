using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Predica.Xrm.Core.DataAccess;
using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSynchronizationApp.Crm.DataAccess
{
    public class ContactRepository : BaseRepository<Contact>, IContactRepository
    {
        public ContactRepository(IOrganizationService service) : base(service)
        {

        }

        public Guid? GetContactByEmail(string email)
        {
            if(email == null)
            {
                return null;
            }

            using (var ctx = new OrganizationServiceContext(this.service))
            {
                return ctx.CreateQuery<Contact>()
                    .Where(x => x.EMailAddress1 == email || x.EMailAddress2 == email || x.EMailAddress3 == email)
                    .Select(x => x.ContactId)
                    .ToList()
                    .FirstOrDefault();
            }
        }

        List<Guid> IContactRepository.GetContactsByEmail(string email)
        {
            if (email == null)
            {
                return null;
            }

            using (var ctx = new OrganizationServiceContext(this.service))
            {
                return ctx.CreateQuery<Contact>()
                    .Where(x => x.EMailAddress1 == email)
                    .Select(x => x.ContactId.Value)
                    .ToList();
            }
        }

        List<Guid> IContactRepository.GetContactsWithSameEmailOrFullNameOrPhone(string email, string firstName, string lastName, string telephone)
        {
            using (var ctx = new OrganizationServiceContext(this.service))
            {
                return ctx.CreateQuery<Contact>()
                    .Where(x => x.EMailAddress1 == email || (x.FirstName == firstName && x.LastName == lastName) || (x.Telephone1 != null && x.Telephone1 == telephone))
                    .Select(x => x.ContactId.Value)
                    .ToList();
            }
        }
    }
}

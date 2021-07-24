using Predica.Xrm.Core.DataAccess;
using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSynchronizationApp.Crm.DataAccess
{
    public interface IContactRepository : IBaseRepository<Contact>
    {
        Guid? GetContactByEmail(string email);
        List<Guid> GetContactsByEmail(string email);
        List<Guid> GetContactsWithSameEmailOrFullNameOrPhone(string email, string firstName, string lastName, string telephone);
    }
}

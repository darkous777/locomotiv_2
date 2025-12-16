using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface ITicketDAL
    {
        IList<Ticket> GetAll();
        Ticket? GetById(int id);
    }
}

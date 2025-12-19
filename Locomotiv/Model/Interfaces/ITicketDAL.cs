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
        void Add(Ticket ticket);
        void Update(Ticket ticket);
        void Delete(int id);
        IList<Ticket> GetTicketsByUser(int userId);
        IList<Ticket> GetTicketsByTrain(int trainId);
    }
}
